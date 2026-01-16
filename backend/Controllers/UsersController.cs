using Microsoft.AspNetCore.Mvc;
using TodoApp.Generated;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ListUsersResponseContent>> ListUsers(
        [FromQuery] int? maxResults = null,
        [FromQuery] string? nextToken = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.ListUsersAsync(maxResults, nextToken, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserOutput>> CreateUser(
        [FromBody] CreateUserRequestContent request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.CreateUserAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserOutput>> GetUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<UserOutput>> GetUserByEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetUserByEmailAsync(email, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserOutput>> UpdateUser(
        string userId,
        [FromBody] UpdateUserRequestContent request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.UpdateUserAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult> DeleteUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await _userService.DeleteUserAsync(userId, cancellationToken);
        return Ok();
    }
}
