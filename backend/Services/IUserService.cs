using TodoApp.DTOs;

namespace TodoApp.Services;

public interface IUserService
{
    Task<ListUsersResponse> ListUsersAsync(double? maxResults, string? nextToken, CancellationToken cancellationToken);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse> GetUserAsync(string userId, CancellationToken cancellationToken);
    Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
}