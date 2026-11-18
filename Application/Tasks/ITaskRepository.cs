using Gestor.Tareas.Api.Rest.Domain.Entities;

namespace Gestor.Tareas.Api.Rest.Application.Tasks;

public interface ITaskRepository
{
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    void Update(TaskItem task);

    void Remove(TaskItem task);
}