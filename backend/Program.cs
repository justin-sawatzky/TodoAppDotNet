using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Middleware;
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
builder.Services.AddDbContext<TodoAppDbContext>(options =>
    options.UseInMemoryDatabase("TodoApp"));

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITodoListService, TodoListService>();
builder.Services.AddScoped<ITodoTaskService, TodoTaskService>();

// Register repositories
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<ITodoListRepository, InMemoryTodoListRepository>();
builder.Services.AddSingleton<ITodoTaskRepository, InMemoryTodoTaskRepository>();

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

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Don't redirect to HTTPS in Docker container
// app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
