using Microsoft.AspNetCore.Mvc;
using TodoApp.Generated;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[Route("users/{userId}/lists")]
public class TodoListsController : ControllerBase
{
    private readonly ITodoListService _todoListService;

    public TodoListsController(ITodoListService todoListService)
    {
        _todoListService = todoListService;
    }

    [HttpGet]
    public async Task<ActionResult<ListTodoListsResponseContent>> ListTodoLists(
        string userId,
        [FromQuery] int? maxResults,
        [FromQuery] string? nextToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _todoListService.ListTodoListsAsync(userId, maxResults, nextToken, cancellationToken);
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

    [HttpPost]
    public async Task<ActionResult<TodoListOutput>> CreateTodoList(
        string userId,
        [FromBody] CreateTodoListRequestContent request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        try
        {
            var result = await _todoListService.CreateTodoListAsync(userId, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResourceNotFoundExceptionResponseContent { Message = ex.Message });
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

    [HttpGet("{listId}")]
    public async Task<ActionResult<TodoListOutput>> GetTodoList(
        string userId,
        string listId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _todoListService.GetTodoListAsync(userId, listId, cancellationToken);
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

    [HttpPut("{listId}")]
    public async Task<ActionResult<TodoListOutput>> UpdateTodoList(
        string userId,
        string listId,
        [FromBody] UpdateTodoListRequestContent request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        try
        {
            var result = await _todoListService.UpdateTodoListAsync(userId, listId, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResourceNotFoundExceptionResponseContent { Message = ex.Message });
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

    [HttpDelete("{listId}")]
    public async Task<ActionResult> DeleteTodoList(
        string userId,
        string listId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _todoListService.DeleteTodoListAsync(userId, listId, cancellationToken);
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
