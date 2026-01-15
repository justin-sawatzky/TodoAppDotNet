import { useState, useEffect } from 'react';
import { api } from '../api/client';
import type { User, TodoList, TodoTask } from '../types';
import { TaskList } from './TaskList';
import './TodoListView.css';

interface TodoListViewProps {
  user: User;
  onLogout: () => void;
}

export function TodoListView({ user, onLogout }: TodoListViewProps) {
  const [lists, setLists] = useState<TodoList[]>([]);
  const [selectedList, setSelectedList] = useState<TodoList | null>(null);
  const [tasks, setTasks] = useState<TodoTask[]>([]);
  const [newListName, setNewListName] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadLists();
  }, [user.userId]);

  useEffect(() => {
    if (selectedList) {
      loadTasks(selectedList.listId);
    }
  }, [selectedList]);

  const loadLists = async () => {
    const { data } = await api.lists.list(user.userId);
    if (data?.lists) {
      setLists(data.lists as TodoList[]);
    }
  };

  const loadTasks = async (listId: string) => {
    const { data } = await api.tasks.list(user.userId, listId);
    if (data?.tasks) {
      setTasks(data.tasks as TodoTask[]);
    }
  };

  const handleCreateList = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newListName.trim()) return;

    setLoading(true);
    const { data } = await api.lists.create(user.userId, newListName);
    if (data) {
      await loadLists();
      setNewListName('');
    }
    setLoading(false);
  };

  const handleSelectList = (list: TodoList) => {
    setSelectedList(list);
  };

  const handleDeleteList = async (listId: string) => {
    if (!confirm('Are you sure you want to delete this list?')) return;
    
    await api.lists.delete(user.userId, listId);
    if (selectedList?.listId === listId) {
      setSelectedList(null);
      setTasks([]);
    }
    await loadLists();
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
