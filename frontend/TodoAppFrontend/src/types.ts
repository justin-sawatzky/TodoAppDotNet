// Re-export types from generated schema to ensure they stay in sync with the API
import type { components } from './api/schema';

export type User = components['schemas']['UserOutput'];
export type TodoList = components['schemas']['TodoListOutput'];
export type TodoTask = components['schemas']['TodoTaskOutput'];
