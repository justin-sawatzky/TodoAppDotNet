using System.Collections.Concurrent;
using TodoApp.Models;

namespace TodoApp.Repositories;

/// <summary>
/// Shared in-memory data store for all repositories.
/// Enables cascade delete operations across entities.
/// </summary>
public class InMemoryDataStore
{
    public ConcurrentDictionary<string, User> Users { get; } = new();
    public ConcurrentDictionary<(string UserId, string ListId), TodoList> TodoLists { get; } = new();
    public ConcurrentDictionary<(string UserId, string ListId, string TaskId), TodoTask> TodoTasks { get; } = new();

    /// <summary>
    /// Deletes all lists and tasks belonging to a user.
    /// </summary>
    public void CascadeDeleteUser(string userId)
    {
        // Delete all tasks for this user
        var taskKeysToRemove = TodoTasks.Keys.Where(k => k.UserId == userId).ToList();
        foreach (var key in taskKeysToRemove)
        {
            TodoTasks.TryRemove(key, out _);
        }

        // Delete all lists for this user
        var listKeysToRemove = TodoLists.Keys.Where(k => k.UserId == userId).ToList();
        foreach (var key in listKeysToRemove)
        {
            TodoLists.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Deletes all tasks belonging to a list.
    /// </summary>
    public void CascadeDeleteList(string userId, string listId)
    {
        var taskKeysToRemove = TodoTasks.Keys
            .Where(k => k.UserId == userId && k.ListId == listId)
            .ToList();

        foreach (var key in taskKeysToRemove)
        {
            TodoTasks.TryRemove(key, out _);
        }
    }
}
