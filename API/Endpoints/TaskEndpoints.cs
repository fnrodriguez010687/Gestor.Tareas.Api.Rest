using Gestor.Tareas.Api.Rest.API.Contracts;
using Gestor.Tareas.Api.Rest.Application.Tasks;

namespace Gestor.Tareas.Api.Rest.API.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/tasks", async (
            ITaskService taskService,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default) =>
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return Results.BadRequest(new
                {
                    message = "Page must be at least 1 and pageSize must be between 1 and 100."
                });
            }

            var result = await taskService.GetPageAsync(page, pageSize, cancellationToken);
            var response = new PagedResponse<TaskResponse>(
                result.Items.Select(TaskResponse.FromEntity).ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount,
                result.TotalPages);

            return Results.Ok(response);
        })
        .WithName("GetTasks");

        return endpoints;
    }
}