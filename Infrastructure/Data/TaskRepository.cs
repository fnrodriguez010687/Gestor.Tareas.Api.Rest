using Gestor.Tareas.Api.Rest.Application.Tasks;
using Gestor.Tareas.Api.Rest.Domain.Entities;
using Gestor.Tareas.Api.Rest.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Gestor.Tareas.Api.Rest.Infrastructure.Data;

public sealed class TaskRepository(AppDbContext dbContext) : ITaskRepository
{
    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        TaskState? status,
        string? assignee,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tasks.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(task => task.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(assignee))
        {
            var normalizedAssignee = assignee.Trim();
            query = query.Where(task => task.Assignee == normalizedAssignee);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var skipCount = ((long)page - 1) * pageSize;
        IReadOnlyList<TaskItem> items = skipCount >= totalCount
            ? Array.Empty<TaskItem>()
            : await query
                .OrderBy(task => task.Id)
                .Skip((int)skipCount)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default) =>
        await dbContext.Tasks.AddAsync(task, cancellationToken);

    public void Update(TaskItem task) => dbContext.Tasks.Update(task);

    public void Remove(TaskItem task) => dbContext.Tasks.Remove(task);
}