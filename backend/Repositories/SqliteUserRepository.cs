using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Repositories;

public class SqliteUserRepository : IUserRepository
{
    private readonly TodoAppDbContext _context;

    public SqliteUserRepository(TodoAppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<User> users, string? nextToken)> GetUsersAsync(int maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        int skip = 0;
        if (!string.IsNullOrEmpty(nextToken) && int.TryParse(nextToken, out var tokenIndex))
        {
            skip = tokenIndex;
        }

        // Use database-side ordering and AsNoTracking for read-only query
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Skip(skip)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // Check if there are more results
        var totalCount = await _context.Users.CountAsync(cancellationToken);
        var hasMore = totalCount > skip + maxResults;
        var nextPageToken = hasMore ? (skip + maxResults).ToString() : null;

        return (users, nextPageToken);
    }

    public async Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await GetUserByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
