using TodoApp.Models;

namespace TodoApp.Repositories;

public class InMemoryTodoTaskRepository : ITodoTaskRepository
{
    private readonly InMemoryDataStore _store;
    private readonly object _orderLock = new();

    public InMemoryTodoTaskRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<(List<TodoTask> tasks, string? nextToken)> GetTasksAsync(string userId, string listId, int maxResults, string? nextToken, bool? completed, CancellationToken cancellationToken)
    {
        var query = _store.TodoTasks.Values
            .Where(t => t.UserId == userId && t.ListId == listId);

        if (completed.HasValue)
        {
            query = query.Where(t => t.Completed == completed.Value);
        }

        var allTasks = query.OrderBy(t => t.Order).ThenBy(t => t.CreatedAt).ToList();

        int skip = 0;
        if (!string.IsNullOrEmpty(nextToken) && int.TryParse(nextToken, out var tokenIndex))
        {
            skip = tokenIndex;
        }

        var tasks = allTasks.Skip(skip).Take(maxResults).ToList();
        var hasMore = allTasks.Count > skip + maxResults;
        var nextPageToken = hasMore ? (skip + maxResults).ToString() : null;

        return Task.FromResult((tasks, nextPageToken));
    }

    public Task<TodoTask?> GetTaskByIdAsync(string userId, string listId, string taskId, CancellationToken cancellationToken)
    {
        _store.TodoTasks.TryGetValue((userId, listId, taskId), out var task);
        return Task.FromResult(task);
    }

    public Task CreateTaskAsync(TodoTask task, int? requestedOrder, CancellationToken cancellationToken)
    {
        // Use lock to prevent race conditions when assigning order
        lock (_orderLock)
        {
            if (requestedOrder.HasValue)
            {
                task.Order = requestedOrder.Value;
            }
            else
            {
                var maxOrder = _store.TodoTasks.Values
                    .Where(t => t.UserId == task.UserId && t.ListId == task.ListId)
                    .Select(t => (int?)t.Order)
                    .Max() ?? -1;

                task.Order = maxOrder + 1;
            }

            _store.TodoTasks.TryAdd((task.UserId, task.ListId, task.TaskId), task);
        }

        return Task.CompletedTask;
    }

    public Task UpdateTaskAsync(TodoTask task, CancellationToken cancellationToken)
    {
        if (_store.TodoTasks.TryGetValue((task.UserId, task.ListId, task.TaskId), out var existingTask))
        {
            _store.TodoTasks.TryUpdate((task.UserId, task.ListId, task.TaskId), task, existingTask);
        }
        return Task.CompletedTask;
    }

    public Task DeleteTaskAsync(string userId, string listId, string taskId, CancellationToken cancellationToken)
    {
        _store.TodoTasks.TryRemove((userId, listId, taskId), out _);
        return Task.CompletedTask;
    }

    public Task<List<TodoTask>> ReorderTasksAsync(string userId, string listId, Dictionary<string, int> taskOrders, CancellationToken cancellationToken)
    {
        lock (_orderLock)
        {
            foreach (var (taskId, newOrder) in taskOrders)
            {
                if (_store.TodoTasks.TryGetValue((userId, listId, taskId), out var task))
                {
                    task.Order = newOrder;
                    task.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            // Return all tasks in the list ordered by new order
            var tasks = _store.TodoTasks.Values
                .Where(t => t.UserId == userId && t.ListId == listId)
                .OrderBy(t => t.Order)
                .ThenBy(t => t.CreatedAt)
                .ToList();

            return Task.FromResult(tasks);
        }
    }
}
