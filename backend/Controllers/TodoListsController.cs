using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.DTOs;
using TodoApp.Generated;
using TodoApp.Models;

namespace TodoApp.Controllers;

[ApiController]
[Route("users/{userId}/lists")]
public class TodoListsController : ControllerBase
{
    private readonly TodoAppDbContext _context;

    public TodoListsController(TodoAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ListTodoListsResponse>> ListTodoLists(
        string userId,
        [FromQuery] int? maxResults,
        [FromQuery] string? nextToken)
    {
        // Load all lists and order on client side for SQLite compatibility
        var lists = await _context.TodoLists
            .Where(l => l.UserId == userId)
            .ToListAsync();

        var orderedLists = lists.OrderBy(l => l.CreatedAt).ToList();

        return Ok(new ListTodoListsResponse
        {
            Lists = orderedLists.Select(MapToResponse).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<TodoListResponse>> CreateTodoList(
        string userId,
        [FromBody] CreateTodoListRequestContent request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        var list = new TodoList
        {
            UserId = userId,
            ListId = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.TodoLists.Add(list);
        await _context.SaveChangesAsync();

        return Ok(MapToResponse(list));
    }

    [HttpGet("{listId}")]
    public async Task<ActionResult<TodoListResponse>> GetTodoList(string userId, string listId)
    {
        var list = await _context.TodoLists
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ListId == listId);

        if (list == null)
        {
            return NotFound(new { message = "List not found" });
        }

        return Ok(MapToResponse(list));
    }

    [HttpPut("{listId}")]
    public async Task<ActionResult<TodoListResponse>> UpdateTodoList(
        string userId,
        string listId,
        [FromBody] UpdateTodoListRequestContent request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse { Message = "Validation failed: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        }

        var list = await _context.TodoLists
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ListId == listId);

        if (list == null)
        {
            return NotFound(new { message = "List not found" });
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            list.Name = request.Name;
        }

        if (request.Description != null)
        {
            list.Description = request.Description;
        }

        list.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToResponse(list));
    }

    [HttpDelete("{listId}")]
    public async Task<IActionResult> DeleteTodoList(string userId, string listId)
    {
        var list = await _context.TodoLists
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ListId == listId);

        if (list == null)
        {
            return NotFound(new { message = "List not found" });
        }

        _context.TodoLists.Remove(list);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private static TodoListResponse MapToResponse(TodoList list) => new()
    {
        UserId = list.UserId,
        ListId = list.ListId,
        Name = list.Name,
        Description = list.Description,
        CreatedAt = list.CreatedAt.ToUnixTimeSeconds(),
        UpdatedAt = list.UpdatedAt.ToUnixTimeSeconds()
    };
}
