using TodoApp.Generated;

namespace TodoApp.Services;

public interface ITodoListService
{
    Task<ListTodoListsResponseContent> ListTodoListsAsync(string userId, int? maxResults, string? nextToken, CancellationToken cancellationToken);
    Task<TodoListOutput> CreateTodoListAsync(string userId, CreateTodoListRequestContent request, CancellationToken cancellationToken);
    Task<TodoListOutput> GetTodoListAsync(string userId, string listId, CancellationToken cancellationToken);
    Task<TodoListOutput> UpdateTodoListAsync(string userId, string listId, UpdateTodoListRequestContent request, CancellationToken cancellationToken);
    Task DeleteTodoListAsync(string userId, string listId, CancellationToken cancellationToken);
}
