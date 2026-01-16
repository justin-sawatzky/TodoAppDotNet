using TodoApp.Models;

namespace TodoApp.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryUserRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<(List<User> users, string? nextToken)> GetUsersAsync(int maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        var allUsers = _store.Users.Values.OrderBy(u => u.CreatedAt).ToList();

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
        _store.Users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = _store.Users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        _store.Users.TryAdd(user.UserId, user);
        return Task.CompletedTask;
    }

    public Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        if (_store.Users.TryGetValue(user.UserId, out var existingUser))
        {
            _store.Users.TryUpdate(user.UserId, user, existingUser);
        }
        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        if (_store.Users.TryRemove(userId, out _))
        {
            // Cascade delete all lists and tasks for this user
            _store.CascadeDeleteUser(userId);
        }
        return Task.CompletedTask;
    }
}
