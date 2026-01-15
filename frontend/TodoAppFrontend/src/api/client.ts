import createClient from 'openapi-fetch';
import type { paths } from './schema';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5247';

export const client = createClient<paths>({ baseUrl: API_BASE_URL });

// Helper functions for common operations
export const api = {
  // User operations
  users: {
    create: (email: string, username: string) =>
      client.POST('/users', { body: { email, username } }),

    list: () => client.GET('/users'),

    get: (userId: string) => client.GET('/users/{userId}', { params: { path: { userId } } }),
  },

  // TodoList operations
  lists: {
    list: (userId: string) => client.GET('/users/{userId}/lists', { params: { path: { userId } } }),

    create: (userId: string, name: string, description?: string) => {
      const body: { name: string; description?: string } = { name };
      if (description !== undefined && description !== '') {
        body.description = description;
      }
      return client.POST('/users/{userId}/lists', {
        params: { path: { userId } },
        body,
      });
    },

    get: (userId: string, listId: string) =>
      client.GET('/users/{userId}/lists/{listId}', {
        params: { path: { userId, listId } },
      }),

    update: (userId: string, listId: string, name?: string, description?: string) =>
      client.PUT('/users/{userId}/lists/{listId}', {
        params: { path: { userId, listId } },
        body: { name, description },
      }),

    delete: (userId: string, listId: string) =>
      client.DELETE('/users/{userId}/lists/{listId}', {
        params: { path: { userId, listId } },
      }),
  },

  // TodoTask operations
  tasks: {
    list: (userId: string, listId: string, completed?: boolean) =>
      client.GET('/users/{userId}/lists/{listId}/tasks', {
        params: { path: { userId, listId }, query: { completed } },
      }),

    create: (userId: string, listId: string, description: string, order?: number) =>
      client.POST('/users/{userId}/lists/{listId}/tasks', {
        params: { path: { userId, listId } },
        body: { description, completed: false, order },
      }),

    get: (userId: string, listId: string, taskId: string) =>
      client.GET('/users/{userId}/lists/{listId}/tasks/{taskId}', {
        params: { path: { userId, listId, taskId } },
      }),

    update: (
      userId: string,
      listId: string,
      taskId: string,
      description?: string,
      completed?: boolean,
      order?: number
    ) =>
      client.PUT('/users/{userId}/lists/{listId}/tasks/{taskId}', {
        params: { path: { userId, listId, taskId } },
        body: { description, completed, order },
      }),

    delete: (userId: string, listId: string, taskId: string) =>
      client.DELETE('/users/{userId}/lists/{listId}/tasks/{taskId}', {
        params: { path: { userId, listId, taskId } },
      }),
  },
};
