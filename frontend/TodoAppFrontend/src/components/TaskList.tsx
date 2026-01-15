import { useState, useEffect } from 'react';
import { api } from '../api/client';
import type { User, TodoList, TodoTask } from '../types';
import './TaskList.css';

interface TaskListProps {
  user: User;
  list: TodoList;
  tasks: TodoTask[];
  onTaskUpdate: () => void;
}

export function TaskList({ user, list, tasks, onTaskUpdate }: TaskListProps) {
  const [newTaskDescription, setNewTaskDescription] = useState('');
  const [editingTaskId, setEditingTaskId] = useState<string | null>(null);
  const [editingDescription, setEditingDescription] = useState('');
  const [draggedTaskId, setDraggedTaskId] = useState<string | null>(null);
  const [orderedTasks, setOrderedTasks] = useState<TodoTask[]>(tasks);

  // Update ordered tasks when tasks prop changes
  useEffect(() => {
    setOrderedTasks(tasks);
  }, [tasks]);

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTaskDescription.trim()) return;

    await api.tasks.create(user.userId, list.listId, newTaskDescription);
    setNewTaskDescription('');
    onTaskUpdate();
  };

  const handleToggleComplete = async (task: TodoTask) => {
    await api.tasks.update(
      user.userId,
      list.listId,
      task.taskId,
      undefined,
      !task.completed
    );
    onTaskUpdate();
  };

  const handleStartEdit = (task: TodoTask) => {
    setEditingTaskId(task.taskId);
    setEditingDescription(task.description);
  };

  const handleSaveEdit = async (taskId: string) => {
    if (!editingDescription.trim()) return;

    await api.tasks.update(
      user.userId,
      list.listId,
      taskId,
      editingDescription,
      undefined
    );
    setEditingTaskId(null);
    onTaskUpdate();
  };

  const handleCancelEdit = () => {
    setEditingTaskId(null);
    setEditingDescription('');
  };

  const handleDeleteTask = async (taskId: string) => {
    await api.tasks.delete(user.userId, list.listId, taskId);
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
      api.tasks.update(
        user.userId,
        list.listId,
        task.taskId,
        undefined,
        undefined,
        index
      )
    );
    
    await Promise.all(updates);
    setDraggedTaskId(null);
    onTaskUpdate();
  };

  return (
    <div className="task-list">
      <div className="task-list-header">
        <h2>{list.name}</h2>
        {list.description && <p className="list-description">{list.description}</p>}
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
                  <span
                    className="task-description"
                    onClick={() => handleStartEdit(task)}
                  >
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
