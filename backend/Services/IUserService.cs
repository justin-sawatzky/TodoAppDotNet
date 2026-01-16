using TodoApp.Generated;

namespace TodoApp.Services;

public interface IUserService
{
    Task<ListUsersResponseContent> ListUsersAsync(double? maxResults, string? nextToken, CancellationToken cancellationToken);
    Task<UserOutput> CreateUserAsync(CreateUserRequestContent request, CancellationToken cancellationToken);
    Task<UserOutput> GetUserAsync(string userId, CancellationToken cancellationToken);
    Task<UserOutput> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserOutput> UpdateUserAsync(string userId, UpdateUserRequestContent request, CancellationToken cancellationToken);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
}
