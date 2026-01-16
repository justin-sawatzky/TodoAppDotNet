import { useState, useEffect } from 'react';
import { api } from '../api/client';
import type { User, TodoList, TodoTask } from '../types';
import { useApiError } from '../hooks/useApiError';
import { ErrorDisplay } from './ErrorDisplay';
import './TaskList.css';

interface TaskListProps {
  user: User;
  list: TodoList;
  tasks: TodoTask[];
  onTaskUpdate: () => void;
  onApiError: (error: unknown, operation: string) => boolean; // Returns true if user was invalidated
  editingListId: string | null;
  editingListName: string;
  editingListDescription: string;
  onStartEditList: (list: TodoList) => void;
  onSaveEditList: (listId: string) => void;
  onCancelEditList: () => void;
  onEditListNameChange: (name: string) => void;
  onEditListDescriptionChange: (description: string) => void;
}

export function TaskList({
  user,
  list,
  tasks,
  onTaskUpdate,
  onApiError,
  editingListId,
  editingListName,
  editingListDescription,
  onStartEditList,
  onSaveEditList,
  onCancelEditList,
  onEditListNameChange,
  onEditListDescriptionChange,
}: TaskListProps) {
  const [newTaskDescription, setNewTaskDescription] = useState('');
  const [editingTaskId, setEditingTaskId] = useState<string | null>(null);
  const [editingDescription, setEditingDescription] = useState('');
  const [draggedTaskId, setDraggedTaskId] = useState<string | null>(null);
  const [orderedTasks, setOrderedTasks] = useState<TodoTask[]>(tasks);
  const { error, handleApiCall, clearError } = useApiError();

  // Update ordered tasks when tasks prop changes
  useEffect(() => {
    setOrderedTasks(tasks);
  }, [tasks]);

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTaskDescription.trim()) return;

    const { error: apiError } = await handleApiCall(
      () => api.tasks.create(user.userId, list.listId, newTaskDescription),
      'create task'
    );

    if (apiError) {
      // Check if user was invalidated
      if (onApiError(apiError, 'create task')) {
        return;
      }
      return;
    }

    setNewTaskDescription('');
    onTaskUpdate();
  };

  const handleToggleComplete = async (task: TodoTask) => {
    const { error: apiError } = await handleApiCall(
      () => api.tasks.update(user.userId, list.listId, task.taskId, undefined, !task.completed),
      'update task'
    );

    if (apiError) {
      if (onApiError(apiError, 'update task')) {
        return;
      }
      return;
    }

    onTaskUpdate();
  };

  const handleStartEdit = (task: TodoTask) => {
    setEditingTaskId(task.taskId);
    setEditingDescription(task.description);
    clearError(); // Clear any existing errors when starting edit
  };

  const handleSaveEdit = async (taskId: string) => {
    if (!editingDescription.trim()) return;

    const { error: apiError } = await handleApiCall(
      () => api.tasks.update(user.userId, list.listId, taskId, editingDescription, undefined),
      'update task'
    );

    if (apiError) {
      if (onApiError(apiError, 'update task')) {
        return;
      }
      return;
    }

    setEditingTaskId(null);
    onTaskUpdate();
  };

  const handleCancelEdit = () => {
    setEditingTaskId(null);
    setEditingDescription('');
    clearError(); // Clear any existing errors when canceling edit
  };

  const handleDeleteTask = async (taskId: string) => {
    const { error: apiError } = await handleApiCall(
      () => api.tasks.delete(user.userId, list.listId, taskId),
      'delete task'
    );

    if (apiError) {
      if (onApiError(apiError, 'delete task')) {
        return;
      }
      return;
    }

    onTaskUpdate();
  };

  const handleDragStart = (taskId: string) => {
    setDraggedTaskId(taskId);
  };

  const handleDragOver = (e: React.DragEvent, targetTaskId: string) => {
    e.preventDefault();
    if (!draggedTaskId || draggedTaskId === targetTaskId) return;

    const draggedIndex = orderedTasks.findIndex((t) => t.taskId === draggedTaskId);
    const targetIndex = orderedTasks.findIndex((t) => t.taskId === targetTaskId);

    if (draggedIndex === -1 || targetIndex === -1) return;

    const newTasks = [...orderedTasks];
    const [draggedTask] = newTasks.splice(draggedIndex, 1);
    newTasks.splice(targetIndex, 0, draggedTask);

    setOrderedTasks(newTasks);
  };

  const handleDragEnd = async () => {
    if (!draggedTaskId) return;

    // Update the order of all tasks based on their new positions
    const updates = orderedTasks.map((task, index) =>
      api.tasks.update(user.userId, list.listId, task.taskId, undefined, undefined, index)
    );

    const { error: apiError } = await handleApiCall(async () => {
      const results = await Promise.all(updates);

      // Check if any of the updates failed
      const hasError = results.some((result) => result.error);
      if (hasError) {
        const firstError = results.find((r) => r.error)?.error;
        throw firstError;
      }

      return { data: results };
    }, 'reorder tasks');

    if (apiError) {
      if (onApiError(apiError, 'reorder tasks')) {
        setDraggedTaskId(null);
        return;
      }
      setDraggedTaskId(null);
      return;
    }

    setDraggedTaskId(null);
    onTaskUpdate();
  };

  return (
    <div className="task-list">
      <div className="task-list-header">
        {editingListId === list.listId ? (
          <div className="list-edit-form">
            <input
              type="text"
              value={editingListName}
              onChange={(e) => onEditListNameChange(e.target.value)}
              placeholder="List name..."
              className="list-name-input"
              autoFocus
            />
            <textarea
              value={editingListDescription}
              onChange={(e) => onEditListDescriptionChange(e.target.value)}
              placeholder="Description (optional)..."
              className="list-description-input"
              rows={3}
            />
            <div className="list-edit-buttons">
              <button onClick={() => onSaveEditList(list.listId)} className="btn-save" title="Save">
                ✓ Save
              </button>
              <button onClick={onCancelEditList} className="btn-cancel" title="Cancel">
                ✕ Cancel
              </button>
            </div>
          </div>
        ) : (
          <>
            <div className="list-header-content">
              <h2>{list.name}</h2>
              <button
                onClick={() => onStartEditList(list)}
                className="btn-edit-list"
                title="Edit list"
              >
                ✎ Edit
              </button>
            </div>
            {list.description && <p className="list-description">{list.description}</p>}
          </>
        )}
      </div>

      <form onSubmit={handleCreateTask} className="new-task-form">
        <input
          type="text"
          value={newTaskDescription}
          onChange={(e) => setNewTaskDescription(e.target.value)}
          placeholder="Add a new task..."
        />
        <button type="submit">Add Task</button>
      </form>

      <ErrorDisplay error={error} onDismiss={clearError} className="compact" />

      <div className="tasks">
        {orderedTasks.length === 0 ? (
          <p className="empty-tasks">No tasks yet. Add one above!</p>
        ) : (
          orderedTasks.map((task) => (
            <div
              key={task.taskId}
              className={`task-item ${task.completed ? 'completed' : ''} ${
                draggedTaskId === task.taskId ? 'dragging' : ''
              }`}
              draggable
              onDragStart={() => handleDragStart(task.taskId)}
              onDragOver={(e) => handleDragOver(e, task.taskId)}
              onDragEnd={handleDragEnd}
            >
              <div className="drag-handle">⋮⋮</div>

              <input
                type="checkbox"
                checked={task.completed}
                onChange={() => handleToggleComplete(task)}
                className="task-checkbox"
              />

              {editingTaskId === task.taskId ? (
                <div className="task-edit">
                  <input
                    type="text"
                    value={editingDescription}
                    onChange={(e) => setEditingDescription(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') handleSaveEdit(task.taskId);
                      if (e.key === 'Escape') handleCancelEdit();
                    }}
                    autoFocus
                  />
                  <button onClick={() => handleSaveEdit(task.taskId)} className="btn-save">
                    ✓
                  </button>
                  <button onClick={handleCancelEdit} className="btn-cancel">
                    ✕
                  </button>
                </div>
              ) : (
                <>
                  <span className="task-description" onClick={() => handleStartEdit(task)}>
                    {task.description}
                  </span>
                  <button
                    onClick={() => handleDeleteTask(task.taskId)}
                    className="btn-delete-task"
                    title="Delete task"
                  >
                    🗑
                  </button>
                </>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
}
