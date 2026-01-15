namespace TodoApp.Models;

public class TodoTask
{
    public string UserId { get; set; } = string.Empty;
    public string ListId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
