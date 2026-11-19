using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gestor.Tareas.Api.Rest.Application.Models;
using Gestor.Tareas.Api.Rest.Application.Tasks;
using Gestor.Tareas.Api.Rest.Domain.Entities;
using Gestor.Tareas.Api.Rest.Domain.Enums;
using Moq;
using Xunit;

namespace Gestor.Tareas.Api.Rest.Tests;

public class TaskServiceTests
{
    [Fact]
    public async Task GetPageAsync_ReturnsPagedResultWithCorrectTotalPages()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var mockUow = new Mock<IUnitOfWork>();
        mockRepo.Setup(r => r.GetPageAsync(1, 2, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskItem> { new(), new() }, 5));
        var service = new TaskService(mockRepo.Object, mockUow.Object);

        // Act
        var result = await service.GetPageAsync(1, 2, null, null);

        // Assert
        Assert.Equal(3, result.TotalPages); // 5/2 = 2.5 => 3
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task CreateAsync_ThrowsIfTitleIsEmpty()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var mockUow = new Mock<IUnitOfWork>();
        var service = new TaskService(mockRepo.Object, mockUow.Object);
        var task = new TaskItem { Title = "", Assignee = "A", DueDate = DateOnly.FromDateTime(DateTime.Today), Status = TaskState.Pendiente };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(task));
        Assert.Contains("Title is required", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsIfStatusIsInvalid()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var mockUow = new Mock<IUnitOfWork>();
        var service = new TaskService(mockRepo.Object, mockUow.Object);
        var task = new TaskItem { Title = "T", Assignee = "A", DueDate = DateOnly.FromDateTime(DateTime.Today), Status = (TaskState)99 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(task));
        Assert.Contains("Status is invalid", exception.Message);
    }
}
