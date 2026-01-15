using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Services;
using TodoApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure Entity Framework with in-memory database (with optional disk persistence for debugging)
var useInMemoryDb = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", true);
var debugSaveToDisk = builder.Configuration.GetValue<bool>("DebugSaveToDisk", false);

if (useInMemoryDb)
{
    builder.Services.AddDbContext<TodoAppDbContext>(options =>
        options.UseInMemoryDatabase("TodoApp"));
}
else
{
    builder.Services.AddDbContext<TodoAppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=todoapp.db"));
}

// Register dependencies
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, SqliteUserRepository>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// For local prototyping - create database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoAppDbContext>();
    context.Database.EnsureCreated();
    
    // If using in-memory database and debug save is enabled, add debug endpoints
    if (useInMemoryDb && debugSaveToDisk)
    {
        // Add a debug endpoint to save in-memory data to SQLite
        app.MapPost("/debug/save-to-disk", async (TodoAppDbContext memoryContext) =>
        {
            // Create a new SQLite context
            var options = new DbContextOptionsBuilder<TodoAppDbContext>()
                .UseSqlite("Data Source=debug_snapshot.db")
                .Options;
                
            using var diskContext = new TodoAppDbContext(options);
            diskContext.Database.EnsureDeleted(); // Delete existing database
            diskContext.Database.EnsureCreated();  // Create fresh database
            
            // Copy all data from memory to disk
            var users = await memoryContext.Users.ToListAsync();
            var lists = await memoryContext.TodoLists.ToListAsync();
            var tasks = await memoryContext.TodoTasks.ToListAsync();
            
            diskContext.Users.AddRange(users);
            diskContext.TodoLists.AddRange(lists);
            diskContext.TodoTasks.AddRange(tasks);
            
            await diskContext.SaveChangesAsync();
            
            return Results.Ok(new { 
                message = "In-memory database saved to debug_snapshot.db",
                users = users.Count,
                lists = lists.Count,
                tasks = tasks.Count
            });
        });
        
        // Add a debug endpoint to load data from disk
        app.MapPost("/debug/load-from-disk", async (TodoAppDbContext memoryContext) =>
        {
            var options = new DbContextOptionsBuilder<TodoAppDbContext>()
                .UseSqlite("Data Source=debug_snapshot.db")
                .Options;
                
            using var diskContext = new TodoAppDbContext(options);
            
            if (!File.Exists("debug_snapshot.db"))
            {
                return Results.BadRequest(new { message = "No debug snapshot found" });
            }
            
            // Clear in-memory database
            memoryContext.TodoTasks.RemoveRange(memoryContext.TodoTasks);
            memoryContext.TodoLists.RemoveRange(memoryContext.TodoLists);
            memoryContext.Users.RemoveRange(memoryContext.Users);
            await memoryContext.SaveChangesAsync();
            
            // Load data from disk
            var users = await diskContext.Users.ToListAsync();
            var lists = await diskContext.TodoLists.ToListAsync();
            var tasks = await diskContext.TodoTasks.ToListAsync();
            
            memoryContext.Users.AddRange(users);
            memoryContext.TodoLists.AddRange(lists);
            memoryContext.TodoTasks.AddRange(tasks);
            
            await memoryContext.SaveChangesAsync();
            
            return Results.Ok(new { 
                message = "Data loaded from debug_snapshot.db to in-memory database",
                users = users.Count,
                lists = lists.Count,
                tasks = tasks.Count
            });
        });
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS for all environments (needed for Docker deployment)
app.UseCors("AllowAll");

// Don't redirect to HTTPS in Docker container
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
