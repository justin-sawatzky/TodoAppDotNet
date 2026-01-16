using TodoApp.Models;

namespace TodoApp.Repositories;

public interface ITodoListRepository
{
    Task<(List<TodoList> lists, string? nextToken)> GetListsAsync(string userId, int maxResults, string? nextToken, CancellationToken cancellationToken);
    Task<TodoList?> GetListByIdAsync(string userId, string listId, CancellationToken cancellationToken);
    Task CreateListAsync(TodoList list, CancellationToken cancellationToken);
    Task UpdateListAsync(TodoList list, CancellationToken cancellationToken);
    Task DeleteListAsync(string userId, string listId, CancellationToken cancellationToken);
}
