using TodoApp.Models;

namespace TodoApp.Repositories;

public interface ITodoTaskRepository
{
    Task<(List<TodoTask> tasks, string? nextToken)> GetTasksAsync(string userId, string listId, int maxResults, string? nextToken, bool? completed, CancellationToken cancellationToken);
    Task<TodoTask?> GetTaskByIdAsync(string userId, string listId, string taskId, CancellationToken cancellationToken);
    /// <summary>
    /// Creates a task with atomic order assignment. If order is null, assigns the next available order.
    /// Uses database locking to prevent race conditions.
    /// </summary>
    Task CreateTaskAsync(TodoTask task, int? requestedOrder, CancellationToken cancellationToken);
    Task UpdateTaskAsync(TodoTask task, CancellationToken cancellationToken);
    Task DeleteTaskAsync(string userId, string listId, string taskId, CancellationToken cancellationToken);
    /// <summary>
    /// Batch update task orders atomically.
    /// </summary>
    Task<List<TodoTask>> ReorderTasksAsync(string userId, string listId, Dictionary<string, int> taskOrders, CancellationToken cancellationToken);
}
