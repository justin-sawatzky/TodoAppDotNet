using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.DTOs;
using TodoApp.Generated;
using TodoApp.Models;

namespace TodoApp.Controllers;

[ApiController]
[Route("users/{userId}/lists/{listId}/tasks")]
public class TodoTasksController : ControllerBase
{
    private readonly TodoAppDbContext _context;

    public TodoTasksController(TodoAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ListTodoTasksResponse>> ListTodoTasks(
        string userId,
        string listId,
        [FromQuery] int? maxResults,
        [FromQuery] string? nextToken,
        [FromQuery] bool? completed)
    {
        var query = _context.TodoTasks
            .Where(t => t.UserId == userId && t.ListId == listId);

        if (completed.HasValue)
        {
            query = query.Where(t => t.Completed == completed.Value);
        }

        // Load all tasks and order by Order field, then by CreatedAt
        var tasks = await query.ToListAsync();
        var orderedTasks = tasks.OrderBy(t => t.Order).ThenBy(t => t.CreatedAt).ToList();

        return Ok(new ListTodoTasksResponse
        {
            Tasks = orderedTasks.Select(MapToResponse).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<TodoTaskResponse>> CreateTodoTask(
        string userId,
        string listId,
        [FromBody] CreateTodoTaskRequestContent request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        // Get the max order value for this list
        var maxOrder = await _context.TodoTasks
            .Where(t => t.UserId == userId && t.ListId == listId)
            .Select(t => (int?)t.Order)
            .MaxAsync() ?? -1;

        var task = new TodoTask
        {
            UserId = userId,
            ListId = listId,
            TaskId = Guid.NewGuid().ToString(),
            Description = request.Description,
            Completed = request.Completed,
            Order = (int)(request.Order == 0 ? (maxOrder + 1) : request.Order),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.TodoTasks.Add(task);
        await _context.SaveChangesAsync();

        return Ok(MapToResponse(task));
    }

    [HttpGet("{taskId}")]
    public async Task<ActionResult<TodoTaskResponse>> GetTodoTask(
        string userId,
        string listId,
        string taskId)
    {
        var task = await _context.TodoTasks
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ListId == listId && t.TaskId == taskId);

        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        return Ok(MapToResponse(task));
    }

    [HttpPut("{taskId}")]
    public async Task<ActionResult<TodoTaskResponse>> UpdateTodoTask(
        string userId,
        string listId,
        string taskId,
        [FromBody] UpdateTodoTaskRequestContent request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        var task = await _context.TodoTasks
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ListId == listId && t.TaskId == taskId);

        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        // For update operations, we only update fields that are provided and not empty/default
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            task.Description = request.Description;
        }

        // For boolean, we only update it if it's provided (not null)
        if (request.Completed.HasValue)
        {
            task.Completed = request.Completed.Value;
        }

        // For order, we only update if it's provided (not null)
        if (request.Order.HasValue)
        {
            task.Order = (int)request.Order.Value;
        }

        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToResponse(task));
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTodoTask(
        string userId,
        string listId,
        string taskId)
    {
        var task = await _context.TodoTasks
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ListId == listId && t.TaskId == taskId);

        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        _context.TodoTasks.Remove(task);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private static TodoTaskResponse MapToResponse(TodoTask task) => new()
    {
        UserId = task.UserId,
        ListId = task.ListId,
        TaskId = task.TaskId,
        Description = task.Description,
        Completed = task.Completed,
        Order = task.Order,
        CreatedAt = task.CreatedAt.ToUnixTimeSeconds(),
        UpdatedAt = task.UpdatedAt.ToUnixTimeSeconds()
    };
}
