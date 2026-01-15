namespace TodoApp.DTOs;

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
}

public class UserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double CreatedAt { get; set; }
}

public class ListUsersResponse
{
    public List<UserResponse> Users { get; set; } = new();
    public string? NextToken { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}