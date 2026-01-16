import createClient from 'openapi-fetch';
import type { paths } from './schema';

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

export const client = createClient<paths>({ baseUrl: API_BASE_URL });

// Helper functions for common operations
export const api = {
  // User operations
  users: {
    create: (email: string, username: string, signal?: AbortSignal) =>
      client.POST('/users', { body: { email, username }, signal }),

    list: (signal?: AbortSignal) => client.GET('/users', { signal }),

    get: (userId: string, signal?: AbortSignal) =>
      client.GET('/users/{userId}', { params: { path: { userId } }, signal }),

    getByEmail: (email: string, signal?: AbortSignal) =>
      client.GET('/users/lookup', { params: { query: { email } }, signal }),
  },

  // TodoList operations
  lists: {
    list: (userId: string, signal?: AbortSignal) =>
      client.GET('/users/{userId}/lists', { params: { path: { userId } }, signal }),

    create: (userId: string, name: string, description?: string, signal?: AbortSignal) => {
      const body: { name: string; description?: string } = { name };
      if (description !== undefined && description !== '') {
        body.description = description;
      }
      return client.POST('/users/{userId}/lists', {
        params: { path: { userId } },
        body,
        signal,
      });
    },

    get: (userId: string, listId: string, signal?: AbortSignal) =>
      client.GET('/users/{userId}/lists/{listId}', {
        params: { path: { userId, listId } },
        signal,
      }),

    update: (
      userId: string,
      listId: string,
      name?: string,
      description?: string,
      signal?: AbortSignal
    ) =>
      client.PUT('/users/{userId}/lists/{listId}', {
        params: { path: { userId, listId } },
        body: { name, description },
        signal,
      }),

    delete: (userId: string, listId: string, signal?: AbortSignal) =>
      client.DELETE('/users/{userId}/lists/{listId}', {
        params: { path: { userId, listId } },
        signal,
      }),
  },

  // TodoTask operations
  tasks: {
    list: (userId: string, listId: string, completed?: boolean, signal?: AbortSignal) =>
      client.GET('/users/{userId}/lists/{listId}/tasks', {
        params: { path: { userId, listId }, query: { completed } },
        signal,
      }),

    create: (
      userId: string,
      listId: string,
      description: string,
      order?: number,
      signal?: AbortSignal
    ) =>
      client.POST('/users/{userId}/lists/{listId}/tasks', {
        params: { path: { userId, listId } },
        body: { description, completed: false, order },
        signal,
      }),

    get: (userId: string, listId: string, taskId: string, signal?: AbortSignal) =>
      client.GET('/users/{userId}/lists/{listId}/tasks/{taskId}', {
        params: { path: { userId, listId, taskId } },
        signal,
      }),

    update: (
      userId: string,
      listId: string,
      taskId: string,
      description?: string,
      completed?: boolean,
      order?: number,
      signal?: AbortSignal
    ) =>
      client.PUT('/users/{userId}/lists/{listId}/tasks/{taskId}', {
        params: { path: { userId, listId, taskId } },
        body: { description, completed, order },
        signal,
      }),

    delete: (userId: string, listId: string, taskId: string, signal?: AbortSignal) =>
      client.DELETE('/users/{userId}/lists/{listId}/tasks/{taskId}', {
        params: { path: { userId, listId, taskId } },
        signal,
      }),

    reorder: (
      userId: string,
      listId: string,
      taskOrders: { taskId: string; order: number }[],
      signal?: AbortSignal
    ) =>
      client.PUT('/users/{userId}/lists/{listId}/tasks/reorder', {
        params: { path: { userId, listId } },
        body: { taskOrders },
        signal,
      }),
  },
};
