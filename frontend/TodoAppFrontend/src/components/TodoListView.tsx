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
  const [editingListId, setEditingListId] = useState<string | null>(null);
  const [editingListName, setEditingListName] = useState('');
  const [editingListDescription, setEditingListDescription] = useState('');
  const [loading, setLoading] = useState(false);
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
      const { data, error: apiError } = await handleApiCall(
        () => api.lists.list(user.userId),
        'load lists'
      );

      if (!isMounted) return;

      if (apiError) {
        handleUserInvalidation(apiError);
        return;
      }

      if (data?.lists) {
        setLists(data.lists as TodoList[]);
      }
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
      const { data, error: apiError } = await handleApiCall(
        () => api.tasks.list(user.userId, selectedList.listId),
        'load tasks'
      );

      if (!isMounted) return;

      if (apiError) {
        handleUserInvalidation(apiError);
        return;
      }

      if (data?.tasks) {
        setTasks(data.tasks as TodoTask[]);
      }
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
    if (!newListName.trim()) return;

    setLoading(true);

    const { data, error: apiError } = await handleApiCall(
      () => api.lists.create(user.userId, newListName),
      'create list'
    );

    if (apiError) {
      setLoading(false);
      return;
    }

    if (data) {
      await loadListsAfterUpdate();
      setNewListName('');
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
    if (!editingListName.trim()) return;

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
    await loadListsAfterUpdate();

    // Update selected list if it's the one being edited
    if (selectedList?.listId === listId) {
      const updatedList = lists.find((l) => l.listId === listId);
      if (updatedList) {
        setSelectedList(updatedList);
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
            <button type="submit" disabled={loading}>
              +
            </button>
          </form>

          <ul className="list-items">
            {lists.map((list) => (
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
            ))}
          </ul>
        </aside>

        <main className="content-area">
          {selectedList ? (
            <TaskList
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
