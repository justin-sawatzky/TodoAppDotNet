import { useState, useEffect } from 'react';
import { api } from '../api/client';
import type { User, TodoList, TodoTask } from '../types';
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
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Helper function to handle API errors
  const handleApiError = (error: any, operation: string) => {
    console.error(`Error during ${operation}:`, error);
    
    // Check if it's a 404 error which might indicate the user doesn't exist
    if (error?.response?.status === 404 || error?.status === 404) {
      setError(`User not found. You will be logged out.`);
      setTimeout(() => {
        onUserInvalidated();
      }, 2000);
      return;
    }
    
    // Handle other API errors
    setError(`Failed to ${operation}. Please try again.`);
    setTimeout(() => setError(null), 5000);
  };

  useEffect(() => {
    loadLists();
  }, [user.userId]);

  useEffect(() => {
    if (selectedList) {
      loadTasks(selectedList.listId);
    }
  }, [selectedList]);

  const loadLists = async () => {
    try {
      const { data, error } = await api.lists.list(user.userId);
      if (error) {
        handleApiError(error, 'load lists');
        return;
      }
      if (data?.lists) {
        setLists(data.lists as TodoList[]);
        setError(null); // Clear any previous errors
      }
    } catch (error) {
      handleApiError(error, 'load lists');
    }
  };

  const loadTasks = async (listId: string) => {
    try {
      const { data, error } = await api.tasks.list(user.userId, listId);
      if (error) {
        handleApiError(error, 'load tasks');
        return;
      }
      if (data?.tasks) {
        setTasks(data.tasks as TodoTask[]);
        setError(null); // Clear any previous errors
      }
    } catch (error) {
      handleApiError(error, 'load tasks');
    }
  };

  const handleCreateList = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newListName.trim()) return;

    setLoading(true);
    try {
      const { data, error } = await api.lists.create(user.userId, newListName);
      if (error) {
        handleApiError(error, 'create list');
        setLoading(false);
        return;
      }
      if (data) {
        await loadLists();
        setNewListName('');
        setError(null); // Clear any previous errors
      }
    } catch (error) {
      handleApiError(error, 'create list');
    }
    setLoading(false);
  };

  const handleSelectList = (list: TodoList) => {
    setSelectedList(list);
  };

  const handleDeleteList = async (listId: string) => {
    if (!confirm('Are you sure you want to delete this list?')) return;
    
    try {
      const { error } = await api.lists.delete(user.userId, listId);
      if (error) {
        handleApiError(error, 'delete list');
        return;
      }
      
      if (selectedList?.listId === listId) {
        setSelectedList(null);
        setTasks([]);
      }
      await loadLists();
      setError(null); // Clear any previous errors
    } catch (error) {
      handleApiError(error, 'delete list');
    }
  };

  const handleTaskUpdate = async () => {
    if (selectedList) {
      await loadTasks(selectedList.listId);
    }
  };

  return (
    <div className="todo-list-view">
      <header className="app-header">
        <h1>Todo App</h1>
        <div className="user-info">
          <span>{user.username}</span>
          <button onClick={onLogout} className="btn-logout">Logout</button>
        </div>
      </header>

      {error && (
        <div className="error-banner" style={{ 
          backgroundColor: '#fee', 
          color: '#c33', 
          padding: '10px', 
          margin: '10px', 
          borderRadius: '4px',
          border: '1px solid #fcc'
        }}>
          {error}
        </div>
      )}

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
            <button type="submit" disabled={loading}>+</button>
          </form>

          <ul className="list-items">
            {lists.map((list) => (
              <li
                key={list.listId}
                className={selectedList?.listId === list.listId ? 'active' : ''}
              >
                <button
                  onClick={() => handleSelectList(list)}
                  className="list-item-button"
                >
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
              onApiError={handleApiError}
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
