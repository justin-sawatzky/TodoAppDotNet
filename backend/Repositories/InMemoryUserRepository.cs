using System.Collections.Concurrent;
using TodoApp.Models;

namespace TodoApp.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public Task<(List<User> users, string? nextToken)> GetUsersAsync(int maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        var allUsers = _users.Values.OrderBy(u => u.CreatedAt).ToList();
        
        int startIndex = 0;
        if (!string.IsNullOrEmpty(nextToken) && int.TryParse(nextToken, out var tokenIndex))
        {
            startIndex = tokenIndex;
        }

        var users = allUsers.Skip(startIndex).Take(maxResults).ToList();
        var hasMore = allUsers.Count > startIndex + maxResults;
        var nextPageToken = hasMore ? (startIndex + maxResults).ToString() : null;

        return Task.FromResult((users, nextPageToken));
    }

    public Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        _users.TryAdd(user.UserId, user);
        return Task.CompletedTask;
    }

    public Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        _users.TryUpdate(user.UserId, user, _users[user.UserId]);
        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        _users.TryRemove(userId, out _);
        return Task.CompletedTask;
    }
}