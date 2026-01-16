using TodoApp.Generated;
using TodoApp.Models;
using TodoApp.Repositories;

namespace TodoApp.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ListUsersResponseContent> ListUsersAsync(int? maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Listing users with maxResults={MaxResults}, nextToken={NextToken}", maxResults, nextToken);

        var (users, nextPageToken) = await _userRepository.GetUsersAsync(
            maxResults ?? 50,
            nextToken,
            cancellationToken);

        _logger.LogDebug("Found {Count} users", users.Count);

        return new ListUsersResponseContent
        {
            Users = users.Select(MapToOutput).ToList(),
            NextToken = nextPageToken
        };
    }

    public async Task<UserOutput> CreateUserAsync(CreateUserRequestContent request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user with username={Username}", request.Username);

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
            _logger.LogWarning("Attempted to create user with existing email");
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

        _logger.LogInformation("Created user with ID={UserId}", user.UserId);

        return MapToOutput(user);
    }

    public async Task<UserOutput> GetUserAsync(string userId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting user with ID={UserId}", userId);

        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogDebug("User not found with ID={UserId}", userId);
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        return MapToOutput(user);
    }

    public async Task<UserOutput> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting user by email");

        var user = await _userRepository.GetUserByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            _logger.LogDebug("User not found by email");
            throw new KeyNotFoundException($"User with email {email} not found");
        }

        return MapToOutput(user);
    }

    public async Task<UserOutput> UpdateUserAsync(string userId, UpdateUserRequestContent request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user with ID={UserId}", userId);

        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Check for email conflict if email is being updated
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("Attempted to update user to existing email");
                throw new InvalidOperationException("User with this email already exists");
            }
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

        _logger.LogInformation("Updated user with ID={UserId}", userId);

        return MapToOutput(user);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user with ID={UserId}", userId);

        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        await _userRepository.DeleteUserAsync(userId, cancellationToken);

        _logger.LogInformation("Deleted user with ID={UserId}", userId);
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
