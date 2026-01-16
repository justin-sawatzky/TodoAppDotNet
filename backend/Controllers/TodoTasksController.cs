using Microsoft.AspNetCore.Mvc;
using TodoApp.Generated;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[Route("users/{userId}/lists/{listId}/tasks")]
public class TodoTasksController : ControllerBase
{
    private readonly ITodoTaskService _todoTaskService;

    public TodoTasksController(ITodoTaskService todoTaskService)
    {
        _todoTaskService = todoTaskService;
    }

    [HttpGet]
    public async Task<ActionResult<ListTodoTasksResponseContent>> ListTodoTasks(
        string userId,
        string listId,
        [FromQuery] int? maxResults,
        [FromQuery] string? nextToken,
        [FromQuery] bool? completed,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _todoTaskService.ListTodoTasksAsync(userId, listId, maxResults, nextToken, completed, cancellationToken);
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
    public async Task<ActionResult<TodoTaskOutput>> CreateTodoTask(
        string userId,
        string listId,
        [FromBody] CreateTodoTaskRequestContent request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        try
        {
            var result = await _todoTaskService.CreateTodoTaskAsync(userId, listId, request, cancellationToken);
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

    [HttpGet("{taskId}")]
    public async Task<ActionResult<TodoTaskOutput>> GetTodoTask(
        string userId,
        string listId,
        string taskId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _todoTaskService.GetTodoTaskAsync(userId, listId, taskId, cancellationToken);
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

    [HttpPut("{taskId}")]
    public async Task<ActionResult<TodoTaskOutput>> UpdateTodoTask(
        string userId,
        string listId,
        string taskId,
        [FromBody] UpdateTodoTaskRequestContent request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        try
        {
            var result = await _todoTaskService.UpdateTodoTaskAsync(userId, listId, taskId, request, cancellationToken);
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

    [HttpDelete("{taskId}")]
    public async Task<ActionResult> DeleteTodoTask(
        string userId,
        string listId,
        string taskId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _todoTaskService.DeleteTodoTaskAsync(userId, listId, taskId, cancellationToken);
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

    [HttpPut("reorder")]
    public async Task<ActionResult<ReorderTodoTasksResponseContent>> ReorderTodoTasks(
        string userId,
        string listId,
        [FromBody] ReorderTodoTasksRequestContent request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationExceptionResponseContent { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        try
        {
            var result = await _todoTaskService.ReorderTodoTasksAsync(userId, listId, request, cancellationToken);
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
}
