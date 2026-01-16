using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Repositories;
using TodoApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure API behavior for automatic model validation
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage);

        var errorResponse = new { Message = "Validation failed: " + string.Join(", ", errors) };
        return new BadRequestObjectResult(errorResponse);
    };
});

builder.Services.AddOpenApi();

// Configure Entity Framework with in-memory database
var useInMemoryDb = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", true);

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

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITodoListService, TodoListService>();
builder.Services.AddScoped<ITodoTaskService, TodoTaskService>();

// Register repositories based on database configuration
if (useInMemoryDb)
{
    builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    builder.Services.AddSingleton<ITodoListRepository, InMemoryTodoListRepository>();
    builder.Services.AddSingleton<ITodoTaskRepository, InMemoryTodoTaskRepository>();
}
else
{
    builder.Services.AddScoped<IUserRepository, SqliteUserRepository>();
    builder.Services.AddScoped<ITodoListRepository, SqliteTodoListRepository>();
    builder.Services.AddScoped<ITodoTaskRepository, SqliteTodoTaskRepository>();
}

var app = builder.Build();

// For local prototyping - create database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoAppDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Don't redirect to HTTPS in Docker container
// app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
