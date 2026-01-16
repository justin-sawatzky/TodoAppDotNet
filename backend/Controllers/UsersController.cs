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
            return StatusCode(500, new ValidationExceptionResponseContent { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserOutput>> CreateUser(
        [FromBody] CreateUserRequestContent request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        try
        {
            var result = await _userService.CreateUserAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ConflictExceptionResponseContent { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ValidationExceptionResponseContent { Message = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserOutput>> GetUser(
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
            return NotFound(new ResourceNotFoundExceptionResponseContent { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ValidationExceptionResponseContent { Message = ex.Message });
        }
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<UserOutput>> GetUserByEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.GetUserByEmailAsync(email, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResourceNotFoundExceptionResponseContent { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ValidationExceptionResponseContent { Message = ex.Message });
        }
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserOutput>> UpdateUser(
        string userId,
        [FromBody] UpdateUserRequestContent request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        try
        {
            var result = await _userService.UpdateUserAsync(userId, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResourceNotFoundExceptionResponseContent { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ConflictExceptionResponseContent { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ValidationExceptionResponseContent { Message = ex.Message });
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
            return NotFound(new ResourceNotFoundExceptionResponseContent { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ValidationExceptionResponseContent { Message = ex.Message });
        }
    }
}
