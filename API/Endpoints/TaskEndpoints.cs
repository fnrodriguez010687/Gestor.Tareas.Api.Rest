using System.ComponentModel.DataAnnotations;
using Gestor.Tareas.Api.Rest.API.Contracts;
using Gestor.Tareas.Api.Rest.Application.Tasks;
using Gestor.Tareas.Api.Rest.Domain.Entities;

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

        endpoints.MapGet("/api/tasks/{id:int}", async (
            int id,
            ITaskService taskService,
            CancellationToken cancellationToken) =>
        {
            var task = await taskService.GetByIdAsync(id, cancellationToken);
            return task is null
                ? Results.NotFound()
                : Results.Ok(TaskResponse.FromEntity(task));
        })
        .WithName("GetTaskById");

        endpoints.MapPost("/api/tasks", async (
            CreateTaskRequest request,
            ITaskService taskService,
            CancellationToken cancellationToken) =>
        {
            var validationErrors = Validate(request);
            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors);
            }

            try
            {
                var task = await taskService.CreateAsync(ToEntity(request), cancellationToken);
                return Results.CreatedAtRoute(
                    "GetTaskById",
                    new { id = task.Id },
                    TaskResponse.FromEntity(task));
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithName("CreateTask");

        endpoints.MapPut("/api/tasks/{id:int}", async (
            int id,
            UpdateTaskRequest request,
            ITaskService taskService,
            CancellationToken cancellationToken) =>
        {
            var validationErrors = Validate(request);
            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors);
            }

            try
            {
                var updated = await taskService.UpdateAsync(id, ToEntity(request), cancellationToken);
                return updated ? Results.NoContent() : Results.NotFound();
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithName("UpdateTask");

        endpoints.MapDelete("/api/tasks/{id:int}", async (
            int id,
            ITaskService taskService,
            CancellationToken cancellationToken) =>
        {
            var deleted = await taskService.DeleteAsync(id, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteTask");

        return endpoints;
    }

    private static Dictionary<string, string[]> Validate<TRequest>(TRequest request)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request!);
        Validator.TryValidateObject(request!, context, validationResults, validateAllProperties: true);

        return validationResults
            .SelectMany(result => result.MemberNames.Select(member => (member, result.ErrorMessage)))
            .GroupBy(error => error.member)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage ?? "The value is invalid.").ToArray());
    }

    private static TaskItem ToEntity(CreateTaskRequest request) => new()
    {
        Title = request.Title,
        Description = request.Description,
        Status = request.Status,
        Assignee = request.Assignee,
        DueDate = request.DueDate
    };

    private static TaskItem ToEntity(UpdateTaskRequest request) => new()
    {
        Title = request.Title,
        Description = request.Description,
        Status = request.Status,
        Assignee = request.Assignee,
        DueDate = request.DueDate
    };
}