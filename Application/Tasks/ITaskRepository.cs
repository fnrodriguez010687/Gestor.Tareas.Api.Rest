using Gestor.Tareas.Api.Rest.Domain.Entities;
using Gestor.Tareas.Api.Rest.Domain.Enums;

namespace Gestor.Tareas.Api.Rest.Application.Tasks;

public interface ITaskRepository
{
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        TaskState? status,
        string? assignee,
        CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    void Update(TaskItem task);

    void Remove(TaskItem task);
}