using Gestor.Tareas.Api.Rest.Data;
using Gestor.Tareas.Api.Rest.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/tasks", async (AppDbContext dbContext, int page = 1, int pageSize = 20) =>
{
    if (page < 1 || pageSize < 1 || pageSize > 100)
    {
        return Results.BadRequest(new
        {
            message = "Page must be at least 1 and pageSize must be between 1 and 100."
        });
    }

    var tasks = dbContext.Tasks.AsNoTracking();
    var totalCount = await tasks.CountAsync();
    var skipCount = ((long)page - 1) * pageSize;
    IReadOnlyList<TaskItem> items = skipCount >= totalCount
        ? Array.Empty<TaskItem>()
        : await tasks
            .OrderBy(task => task.Id)
            .Skip((int)skipCount)
            .Take(pageSize)
            .ToListAsync();

    return Results.Ok(new PagedResponse<TaskItem>(
        items,
        page,
        pageSize,
        totalCount,
        (int)Math.Ceiling(totalCount / (double)pageSize)));
})
.WithName("GetTasks");

app.Run();

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
