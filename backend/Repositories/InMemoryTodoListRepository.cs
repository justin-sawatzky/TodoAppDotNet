using TodoApp.Models;

namespace TodoApp.Repositories;

public class InMemoryTodoListRepository : ITodoListRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryTodoListRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<(List<TodoList> lists, string? nextToken)> GetListsAsync(string userId, int maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        var allLists = _store.TodoLists.Values
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.CreatedAt)
            .ToList();

        int skip = 0;
        if (!string.IsNullOrEmpty(nextToken) && int.TryParse(nextToken, out var tokenIndex))
        {
            skip = tokenIndex;
        }

        var lists = allLists.Skip(skip).Take(maxResults).ToList();
        var hasMore = allLists.Count > skip + maxResults;
        var nextPageToken = hasMore ? (skip + maxResults).ToString() : null;

        return Task.FromResult((lists, nextPageToken));
    }

    public Task<TodoList?> GetListByIdAsync(string userId, string listId, CancellationToken cancellationToken)
    {
        _store.TodoLists.TryGetValue((userId, listId), out var list);
        return Task.FromResult(list);
    }

    public Task CreateListAsync(TodoList list, CancellationToken cancellationToken)
    {
        _store.TodoLists.TryAdd((list.UserId, list.ListId), list);
        return Task.CompletedTask;
    }

    public Task UpdateListAsync(TodoList list, CancellationToken cancellationToken)
    {
        if (_store.TodoLists.TryGetValue((list.UserId, list.ListId), out var existingList))
        {
            _store.TodoLists.TryUpdate((list.UserId, list.ListId), list, existingList);
        }
        return Task.CompletedTask;
    }

    public Task DeleteListAsync(string userId, string listId, CancellationToken cancellationToken)
    {
        if (_store.TodoLists.TryRemove((userId, listId), out _))
        {
            // Cascade delete all tasks for this list
            _store.CascadeDeleteList(userId, listId);
        }
        return Task.CompletedTask;
    }
}
