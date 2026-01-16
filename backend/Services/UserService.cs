using TodoApp.Generated;
using TodoApp.Models;
using TodoApp.Repositories;

namespace TodoApp.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ListUsersResponseContent> ListUsersAsync(double? maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        var (users, nextPageToken) = await _userRepository.GetUsersAsync(
            (int?)maxResults ?? 50,
            nextToken,
            cancellationToken);

        return new ListUsersResponseContent
        {
            Users = users.Select(MapToOutput).ToList(),
            NextToken = nextPageToken
        };
    }

    public async Task<UserOutput> CreateUserAsync(CreateUserRequestContent request, CancellationToken cancellationToken)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username is required");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required");
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        var user = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.CreateUserAsync(user, cancellationToken);

        return MapToOutput(user);
    }

    public async Task<UserOutput> GetUserAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        return MapToOutput(user);
    }

    public async Task<UserOutput> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {email} not found");
        }

        return MapToOutput(user);
    }

    public async Task<UserOutput> UpdateUserAsync(string userId, UpdateUserRequestContent request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            user.Username = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email;
        }

        await _userRepository.UpdateUserAsync(user, cancellationToken);

        return MapToOutput(user);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        await _userRepository.DeleteUserAsync(userId, cancellationToken);
    }

    // Mapping methods
    private static UserOutput MapToOutput(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        CreatedAt = user.CreatedAt.ToUnixTimeSeconds()
    };
}
