using Microsoft.AspNetCore.Mvc;
using TodoApp.DTOs;
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
    public async Task<ActionResult<ListUsersResponse>> ListUsers(
        [FromQuery] double? maxResults = null,
        [FromQuery] string? nextToken = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.ListUsersAsync(maxResults, nextToken, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.CreateUserAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserResponse>> GetUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.GetUserAsync(userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(
        string userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.UpdateUserAsync(userId, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult> DeleteUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _userService.DeleteUserAsync(userId, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message });
        }
    }
}