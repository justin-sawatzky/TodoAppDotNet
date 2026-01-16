using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Repositories;

public class SqliteTodoListRepository : ITodoListRepository
{
    private readonly TodoAppDbContext _context;

    public SqliteTodoListRepository(TodoAppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<TodoList> lists, string? nextToken)> GetListsAsync(string userId, int maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        int skip = 0;
        if (!string.IsNullOrEmpty(nextToken) && int.TryParse(nextToken, out var tokenIndex))
        {
            skip = tokenIndex;
        }

        // Use database-side ordering and AsNoTracking for read-only query
        var lists = await _context.TodoLists
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.CreatedAt)
            .Skip(skip)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // Check if there are more results
        var totalCount = await _context.TodoLists
            .Where(l => l.UserId == userId)
            .CountAsync(cancellationToken);
        var hasMore = totalCount > skip + maxResults;
        var nextPageToken = hasMore ? (skip + maxResults).ToString() : null;

        return (lists, nextPageToken);
    }

    public async Task<TodoList?> GetListByIdAsync(string userId, string listId, CancellationToken cancellationToken)
    {
        return await _context.TodoLists
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ListId == listId, cancellationToken);
    }

    public async Task CreateListAsync(TodoList list, CancellationToken cancellationToken)
    {
        _context.TodoLists.Add(list);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateListAsync(TodoList list, CancellationToken cancellationToken)
    {
        _context.TodoLists.Update(list);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteListAsync(string userId, string listId, CancellationToken cancellationToken)
    {
        var list = await GetListByIdAsync(userId, listId, cancellationToken);
        if (list != null)
        {
            _context.TodoLists.Remove(list);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
