using Gestor.Tareas.Api.Rest.Application.Models;
using Gestor.Tareas.Api.Rest.Domain.Entities;

namespace Gestor.Tareas.Api.Rest.Application.Tasks;

public interface ITaskService
{
    Task<PagedResult<TaskItem>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}