using TodoApp.Models;

namespace TodoApp.Repositories;

public interface IUserRepository
{
    Task<(List<User> users, string? nextToken)> GetUsersAsync(int maxResults, string? nextToken, CancellationToken cancellationToken);
    Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task CreateUserAsync(User user, CancellationToken cancellationToken);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
}