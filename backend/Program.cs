using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Services;
using TodoApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<TodoAppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=todoapp.db"));

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
