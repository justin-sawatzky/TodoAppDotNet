using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Repositories;

public class SqliteTodoTaskRepository : ITodoTaskRepository
{
    private readonly TodoAppDbContext _context;

    public SqliteTodoTaskRepository(TodoAppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<TodoTask> tasks, string? nextToken)> GetTasksAsync(string userId, string listId, int maxResults, string? nextToken, bool? completed, CancellationToken cancellationToken)
    {
        int skip = 0;
        if (!string.IsNullOrEmpty(nextToken) && int.TryParse(nextToken, out var tokenIndex))
        {
            skip = tokenIndex;
        }

        var query = _context.TodoTasks
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.ListId == listId);

        if (completed.HasValue)
        {
            query = query.Where(t => t.Completed == completed.Value);
        }

        // Use database-side ordering
        var tasks = await query
            .OrderBy(t => t.Order)
            .ThenBy(t => t.CreatedAt)
            .Skip(skip)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // Check if there are more results
        var countQuery = _context.TodoTasks
            .Where(t => t.UserId == userId && t.ListId == listId);
        if (completed.HasValue)
        {
            countQuery = countQuery.Where(t => t.Completed == completed.Value);
        }
        var totalCount = await countQuery.CountAsync(cancellationToken);
        var hasMore = totalCount > skip + maxResults;
        var nextPageToken = hasMore ? (skip + maxResults).ToString() : null;

        return (tasks, nextPageToken);
    }

    public async Task<TodoTask?> GetTaskByIdAsync(string userId, string listId, string taskId, CancellationToken cancellationToken)
    {
        return await _context.TodoTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ListId == listId && t.TaskId == taskId, cancellationToken);
    }

    public async Task CreateTaskAsync(TodoTask task, int? requestedOrder, CancellationToken cancellationToken)
    {
        // Use a transaction with serializable isolation to prevent race conditions
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                // Get max order within the transaction
                var maxOrder = await _context.TodoTasks
                    .Where(t => t.UserId == task.UserId && t.ListId == task.ListId)
                    .Select(t => (int?)t.Order)
                    .MaxAsync(cancellationToken) ?? -1;

                // Assign order: use requested order if provided, otherwise next available
                task.Order = requestedOrder ?? (maxOrder + 1);

                _context.TodoTasks.Add(task);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task UpdateTaskAsync(TodoTask task, CancellationToken cancellationToken)
    {
        _context.TodoTasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTaskAsync(string userId, string listId, string taskId, CancellationToken cancellationToken)
    {
        var task = await GetTaskByIdAsync(userId, listId, taskId, cancellationToken);
        if (task != null)
        {
            _context.TodoTasks.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<TodoTask>> ReorderTasksAsync(string userId, string listId, Dictionary<string, int> taskOrders, CancellationToken cancellationToken)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                var tasks = await _context.TodoTasks
                    .Where(t => t.UserId == userId && t.ListId == listId && taskOrders.Keys.Contains(t.TaskId))
                    .ToListAsync(cancellationToken);

                foreach (var task in tasks)
                {
                    if (taskOrders.TryGetValue(task.TaskId, out var newOrder))
                    {
                        task.Order = newOrder;
                        task.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Return all tasks in the list ordered by new order
                return await _context.TodoTasks
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.ListId == listId)
                    .OrderBy(t => t.Order)
                    .ThenBy(t => t.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
