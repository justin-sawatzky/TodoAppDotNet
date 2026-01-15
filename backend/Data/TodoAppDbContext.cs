using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class TodoAppDbContext : DbContext
{
    public TodoAppDbContext(DbContextOptions<TodoAppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<TodoList> TodoLists { get; set; }
    public DbSet<TodoTask> TodoTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // TodoList configuration
        modelBuilder.Entity<TodoList>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ListId });
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ListId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Foreign key relationship
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TodoTask configuration
        modelBuilder.Entity<TodoTask>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ListId, e.TaskId });
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ListId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TaskId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Completed).IsRequired();
            entity.Property(e => e.Order).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Foreign key relationship
            entity.HasOne<TodoList>()
                .WithMany()
                .HasForeignKey(e => new { e.UserId, e.ListId })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
