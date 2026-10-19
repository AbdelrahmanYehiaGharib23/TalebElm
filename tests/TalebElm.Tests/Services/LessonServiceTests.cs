using Moq;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.Services;

public class LessonServiceTests
{
    [Fact]
    public async Task GetByModuleAsync_ReturnsLessonsInRepositoryOrder()
    {
        // Arrange
        var moduleId = Guid.NewGuid();

        var lessons = new List<Lesson>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Lesson 2",
                Content = "Second lesson",
                Order = 2,
                ModuleId = moduleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Lesson 1",
                Content = "First lesson",
                Order = 1,
                ModuleId = moduleId
            }
        };

        var lessonsMock = new Mock<ILessonRepository>();

        lessonsMock
            .Setup(repository => repository.GetByModuleIdAsync(moduleId))
            .ReturnsAsync(lessons);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.Lessons)
            .Returns(lessonsMock.Object);

        var service = new LessonService(unitOfWorkMock.Object);

        // Act
        var result = await service.GetByModuleAsync(moduleId);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("Lesson 2", result[0].Title);
        Assert.Equal(moduleId, result[0].ModuleId);

        Assert.Equal("Lesson 1", result[1].Title);
        Assert.Equal(moduleId, result[1].ModuleId);

        lessonsMock.Verify(
            repository => repository.GetByModuleIdAsync(moduleId),
            Times.Once);
    }

    [Fact]
    public async Task GetByModuleAsync_WhenNoLessonsExist_ReturnsEmptyList()
    {
        // Arrange
        var moduleId = Guid.NewGuid();

        var lessonsMock = new Mock<ILessonRepository>();

        lessonsMock
            .Setup(repository => repository.GetByModuleIdAsync(moduleId))
            .ReturnsAsync(new List<Lesson>());

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.Lessons)
            .Returns(lessonsMock.Object);

        var service = new LessonService(unitOfWorkMock.Object);

        // Act
        var result = await service.GetByModuleAsync(moduleId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLessonExists_ReturnsLessonDetail()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        var lesson = new Lesson
        {
            Id = lessonId,
            Title = "C# Basics",
            Content = "Introduction to C#",
            Order = 3,
            ModuleId = moduleId
        };

        var lessonsMock = new Mock<ILessonRepository>();

        lessonsMock
            .Setup(repository => repository.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.Lessons)
            .Returns(lessonsMock.Object);

        var service = new LessonService(unitOfWorkMock.Object);

        // Act
        var result = await service.GetByIdAsync(lessonId);

        // Assert
        Assert.Equal(lessonId, result.Id);
        Assert.Equal(moduleId, result.ModuleId);
        Assert.Equal("C# Basics", result.Title);
        Assert.Equal("Introduction to C#", result.Content);
        Assert.Equal(3, result.Order);

        // Resource loading belongs to the separate LessonResource work.
        Assert.Empty(result.Resources);

        lessonsMock.Verify(
            repository => repository.GetByIdAsync(lessonId),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var lessonId = Guid.NewGuid();

        var lessonsMock = new Mock<ILessonRepository>();

        lessonsMock
            .Setup(repository => repository.GetByIdAsync(lessonId))
            .ReturnsAsync((Lesson?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.Lessons)
            .Returns(lessonsMock.Object);

        var service = new LessonService(unitOfWorkMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIdAsync(lessonId));
    }
}