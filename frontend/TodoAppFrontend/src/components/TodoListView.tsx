import { useState, useEffect, useCallback } from 'react';
import { api } from '../api/client';
import type { User, TodoList, TodoTask } from '../types';
import { useApiError } from '../hooks/useApiError';
import { ErrorDisplay } from './ErrorDisplay';
import { TaskList } from './TaskList';
import './TodoListView.css';

interface TodoListViewProps {
  user: User;
  onLogout: () => void;
  onUserInvalidated: () => void;
}

export function TodoListView({ user, onLogout, onUserInvalidated }: TodoListViewProps) {
  const [lists, setLists] = useState<TodoList[]>([]);
  const [selectedList, setSelectedList] = useState<TodoList | null>(null);
  const [tasks, setTasks] = useState<TodoTask[]>([]);
  const [newListName, setNewListName] = useState('');
  const [newListDescription, setNewListDescription] = useState('');
  const [editingListId, setEditingListId] = useState<string | null>(null);
  const [editingListName, setEditingListName] = useState('');
  const [editingListDescription, setEditingListDescription] = useState('');
  const [loading, setLoading] = useState(false);
  const [loadingLists, setLoadingLists] = useState(true);
  const [loadingTasks, setLoadingTasks] = useState(false);
  const { error, handleApiCall, clearError, handleError } = useApiError();

  // Helper function to handle user invalidation
  const handleUserInvalidation = useCallback(
    (error: unknown) => {
      if (
        error &&
        typeof error === 'object' &&
        ('type' in error
          ? error.type === 'not_found'
          : 'response' in error && typeof error.response === 'object' && error.response !== null
            ? 'status' in error.response && error.response.status === 404
            : 'status' in error && error.status === 404)
      ) {
        setTimeout(() => {
          onUserInvalidated();
        }, 2000);
        return true;
      }
      return false;
    },
    [onUserInvalidated]
  );

  useEffect(() => {
    let isMounted = true;

    const loadLists = async () => {
      setLoadingLists(true);
      const { data, error: apiError } = await handleApiCall(
        () => api.lists.list(user.userId),
        'load lists'
      );

      if (!isMounted) return;

      if (apiError) {
        handleUserInvalidation(apiError);
        setLoadingLists(false);
        return;
      }

      if (data?.lists) {
        setLists(data.lists as TodoList[]);
      }
      setLoadingLists(false);
    };

    loadLists();

    return () => {
      isMounted = false;
    };
  }, [user.userId, handleApiCall, handleUserInvalidation]);

  useEffect(() => {
    if (!selectedList) return;

    let isMounted = true;

    const loadTasks = async () => {
      setLoadingTasks(true);
      const { data, error: apiError } = await handleApiCall(
        () => api.tasks.list(user.userId, selectedList.listId),
        'load tasks'
      );

      if (!isMounted) return;

      if (apiError) {
        handleUserInvalidation(apiError);
        setLoadingTasks(false);
        return;
      }

      if (data?.tasks) {
        setTasks(data.tasks as TodoTask[]);
      }
      setLoadingTasks(false);
    };

    loadTasks();

    return () => {
      isMounted = false;
    };
  }, [selectedList, user.userId, handleApiCall, handleUserInvalidation]);

  const loadListsAfterUpdate = async () => {
    const { data, error: apiError } = await handleApiCall(
      () => api.lists.list(user.userId),
      'load lists'
    );

    if (apiError) {
      handleUserInvalidation(apiError);
      return;
    }

    if (data?.lists) {
      setLists(data.lists as TodoList[]);
    }
  };

  const loadTasksAfterUpdate = async () => {
    if (!selectedList) return;

    const { data, error: apiError } = await handleApiCall(
      () => api.tasks.list(user.userId, selectedList.listId),
      'load tasks'
    );

    if (apiError) {
      handleUserInvalidation(apiError);
      return;
    }

    if (data?.tasks) {
      setTasks(data.tasks as TodoTask[]);
    }
  };

  const handleCreateList = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validate list name
    if (!newListName.trim()) {
      if (newListName.length > 0) {
        // Has content but only whitespace
        handleError({ message: 'List name cannot be empty or contain only spaces' }, 'create list');
      }
      return;
    }

    setLoading(true);

    const { data, error: apiError } = await handleApiCall(
      () => api.lists.create(user.userId, newListName, newListDescription.trim() || undefined),
      'create list'
    );

    if (apiError) {
      setLoading(false);
      return;
    }

    if (data) {
      await loadListsAfterUpdate();
      setNewListName('');
      setNewListDescription('');
    }

    setLoading(false);
  };

  const handleSelectList = (list: TodoList) => {
    setSelectedList(list);
    clearError(); // Clear any existing errors when switching lists
  };

  const handleDeleteList = async (listId: string) => {
    if (!confirm('Are you sure you want to delete this list?')) return;

    const { error: apiError } = await handleApiCall(
      () => api.lists.delete(user.userId, listId),
      'delete list'
    );

    if (apiError) {
      return;
    }

    if (selectedList?.listId === listId) {
      setSelectedList(null);
      setTasks([]);
    }
    await loadListsAfterUpdate();
  };

  const handleStartEditList = (list: TodoList) => {
    setEditingListId(list.listId);
    setEditingListName(list.name);
    setEditingListDescription(list.description || '');
    clearError();
  };

  const handleSaveEditList = async (listId: string) => {
    // Validate list name
    if (!editingListName.trim()) {
      if (editingListName.length > 0) {
        // Has content but only whitespace
        handleError({ message: 'List name cannot be empty or contain only spaces' }, 'update list');
      }
      return;
    }

    const { error: apiError } = await handleApiCall(
      () =>
        api.lists.update(
          user.userId,
          listId,
          editingListName,
          editingListDescription.trim() || undefined
        ),
      'update list'
    );

    if (apiError) {
      return;
    }

    setEditingListId(null);

    // Reload lists and update selected list if needed
    const { data, error: reloadError } = await handleApiCall(
      () => api.lists.list(user.userId),
      'load lists'
    );

    if (reloadError) {
      handleUserInvalidation(reloadError);
      return;
    }

    if (data?.lists) {
      const updatedLists = data.lists as TodoList[];
      setLists(updatedLists);

      // Update selected list if it's the one being edited
      // Force a new object reference to ensure React detects the change
      if (selectedList?.listId === listId) {
        const updatedList = updatedLists.find((l) => l.listId === listId);
        if (updatedList) {
          setSelectedList({ ...updatedList });
        }
      }
    }
  };

  const handleCancelEditList = () => {
    setEditingListId(null);
    setEditingListName('');
    setEditingListDescription('');
    clearError();
  };

  const handleTaskUpdate = async () => {
    await loadTasksAfterUpdate();
  };

  // Pass error handling to TaskList component
  const handleTaskError = (apiError: unknown, operation: string) => {
    const parsedError = handleError(apiError, operation);
    return handleUserInvalidation(parsedError);
  };

  return (
    <div className="todo-list-view">
      <header className="app-header">
        <h1>Todo App</h1>
        <div className="user-info">
          <span>{user.username}</span>
          <button onClick={onLogout} className="btn-logout">
            Logout
          </button>
        </div>
      </header>

      <ErrorDisplay error={error} onDismiss={clearError} />

      <div className="main-content">
        <aside className="sidebar">
          <div className="sidebar-header">
            <h2>My Lists</h2>
          </div>

          <form onSubmit={handleCreateList} className="new-list-form">
            <input
              type="text"
              value={newListName}
              onChange={(e) => setNewListName(e.target.value)}
              placeholder="New list name..."
              disabled={loading}
            />
            <textarea
              value={newListDescription}
              onChange={(e) => setNewListDescription(e.target.value)}
              placeholder="Description (optional)..."
              disabled={loading}
              rows={2}
              className="new-list-description"
            />
            <button type="submit" disabled={loading}>
              + Add List
            </button>
          </form>

          <ul className="list-items">
            {loadingLists ? (
              <li className="loading-state">Loading lists...</li>
            ) : lists.length === 0 ? (
              <li className="empty-list-state">No lists yet. Create one above!</li>
            ) : (
              lists.map((list) => (
                <li
                  key={list.listId}
                  className={selectedList?.listId === list.listId ? 'active' : ''}
                >
                  <button onClick={() => handleSelectList(list)} className="list-item-button">
                    {list.name}
                  </button>
                  <button
                    onClick={() => handleDeleteList(list.listId)}
                    className="btn-delete"
                    title="Delete list"
                  >
                    ×
                  </button>
                </li>
              ))
            )}
          </ul>
        </aside>

        <main className="content-area">
          {selectedList ? (
            loadingTasks ? (
              <div className="loading-state">Loading tasks...</div>
            ) : (
              <TaskList
                key={`${selectedList.listId}-${selectedList.updatedAt}`}
                user={user}
                list={selectedList}
                tasks={tasks}
                onTaskUpdate={handleTaskUpdate}
                onApiError={handleTaskError}
                editingListId={editingListId}
                editingListName={editingListName}
                editingListDescription={editingListDescription}
                onStartEditList={handleStartEditList}
                onSaveEditList={handleSaveEditList}
                onCancelEditList={handleCancelEditList}
                onEditListNameChange={setEditingListName}
                onEditListDescriptionChange={setEditingListDescription}
              />
            )
          ) : (
            <div className="empty-state">
              <p>Select a list to view tasks</p>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
