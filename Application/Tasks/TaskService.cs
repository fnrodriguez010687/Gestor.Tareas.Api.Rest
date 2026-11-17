using Gestor.Tareas.Api.Rest.Application.Models;
using Gestor.Tareas.Api.Rest.Domain.Entities;

namespace Gestor.Tareas.Api.Rest.Application.Tasks;

public sealed class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public async Task<PagedResult<TaskItem>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await taskRepository.GetPageAsync(page, pageSize, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<TaskItem>(items, page, pageSize, totalCount, totalPages);
    }
}