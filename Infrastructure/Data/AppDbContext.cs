using Gestor.Tareas.Api.Rest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gestor.Tareas.Api.Rest.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var task = modelBuilder.Entity<TaskItem>();

        task.HasKey(item => item.Id);
        task.Property(item => item.Title).HasMaxLength(200).IsRequired();
        task.Property(item => item.Description).HasMaxLength(2000);
        task.Property(item => item.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        task.Property(item => item.Assignee).HasMaxLength(120).IsRequired();
        task.Property(item => item.DueDate).IsRequired();
    }
}