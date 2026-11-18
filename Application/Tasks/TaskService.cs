using Gestor.Tareas.Api.Rest.Application.Models;
using Gestor.Tareas.Api.Rest.Domain.Entities;
using Gestor.Tareas.Api.Rest.Domain.Enums;

namespace Gestor.Tareas.Api.Rest.Application.Tasks;

public sealed class TaskService(ITaskRepository taskRepository, IUnitOfWork unitOfWork) : ITaskService
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

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        taskRepository.GetByIdAsync(id, cancellationToken);

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        Validate(task);
        await taskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<bool> UpdateAsync(int id, TaskItem task, CancellationToken cancellationToken = default)
    {
        Validate(task);
        var existingTask = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (existingTask is null)
        {
            return false;
        }

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.Assignee = task.Assignee;
        existingTask.DueDate = task.DueDate;
        taskRepository.Update(existingTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        taskRepository.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Title))
        {
            throw new ArgumentException("Title is required.", nameof(task));
        }

        if (task.Title.Length > 200)
        {
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(task));
        }

        if (task.Description?.Length > 2000)
        {
            throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(task));
        }

        if (!Enum.IsDefined(task.Status))
        {
            throw new ArgumentException("Status is invalid.", nameof(task));
        }

        if (string.IsNullOrWhiteSpace(task.Assignee))
        {
            throw new ArgumentException("Assignee is required.", nameof(task));
        }

        if (task.Assignee.Length > 120)
        {
            throw new ArgumentException("Assignee cannot exceed 120 characters.", nameof(task));
        }

        if (task.DueDate == default)
        {
            throw new ArgumentException("Due date is required.", nameof(task));
        }
    }
}