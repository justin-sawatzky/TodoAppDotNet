namespace TodoApp.Models;

public class TodoList
{
    public string UserId { get; set; } = string.Empty;
    public string ListId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}