using TodoApp.Generated;

namespace TodoApp.Services;

public interface ITodoTaskService
{
    Task<ListTodoTasksResponseContent> ListTodoTasksAsync(string userId, string listId, int? maxResults, string? nextToken, bool? completed, CancellationToken cancellationToken);
    Task<TodoTaskOutput> CreateTodoTaskAsync(string userId, string listId, CreateTodoTaskRequestContent request, CancellationToken cancellationToken);
    Task<TodoTaskOutput> GetTodoTaskAsync(string userId, string listId, string taskId, CancellationToken cancellationToken);
    Task<TodoTaskOutput> UpdateTodoTaskAsync(string userId, string listId, string taskId, UpdateTodoTaskRequestContent request, CancellationToken cancellationToken);
    Task DeleteTodoTaskAsync(string userId, string listId, string taskId, CancellationToken cancellationToken);
    Task<ReorderTodoTasksResponseContent> ReorderTodoTasksAsync(string userId, string listId, ReorderTodoTasksRequestContent request, CancellationToken cancellationToken);
}
