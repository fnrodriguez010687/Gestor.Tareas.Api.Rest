using Gestor.Tareas.Api.Rest.Application.Models;
using Gestor.Tareas.Api.Rest.Domain.Entities;

namespace Gestor.Tareas.Api.Rest.Application.Tasks;

public interface ITaskService
{
    Task<PagedResult<TaskItem>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, TaskItem task, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}