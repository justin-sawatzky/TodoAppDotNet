export interface User {
  userId: string;
  username: string;
  email: string;
  createdAt: number;
}

export interface TodoList {
  userId: string;
  listId: string;
  name: string;
  description?: string;
  createdAt: number;
  updatedAt: number;
}

export interface TodoTask {
  userId: string;
  listId: string;
  taskId: string;
  description: string;
  completed: boolean;
  order: number;
  createdAt: number;
  updatedAt: number;
}
