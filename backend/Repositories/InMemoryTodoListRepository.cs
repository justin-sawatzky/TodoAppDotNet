using System.Collections.Concurrent;
using TodoApp.Models;

namespace TodoApp.Repositories;

public class InMemoryTodoListRepository : ITodoListRepository
{
    private readonly ConcurrentDictionary<(string UserId, string ListId), TodoList> _lists = new();

    public Task<(List<TodoList> lists, string? nextToken)> GetListsAsync(string userId, int maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        var allLists = _lists.Values
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
        _lists.TryGetValue((userId, listId), out var list);
        return Task.FromResult(list);
    }

    public Task CreateListAsync(TodoList list, CancellationToken cancellationToken)
    {
        _lists.TryAdd((list.UserId, list.ListId), list);
        return Task.CompletedTask;
    }

    public Task UpdateListAsync(TodoList list, CancellationToken cancellationToken)
    {
        _lists.TryUpdate((list.UserId, list.ListId), list, _lists[(list.UserId, list.ListId)]);
        return Task.CompletedTask;
    }

    public Task DeleteListAsync(string userId, string listId, CancellationToken cancellationToken)
    {
        _lists.TryRemove((userId, listId), out _);
        return Task.CompletedTask;
    }
}
