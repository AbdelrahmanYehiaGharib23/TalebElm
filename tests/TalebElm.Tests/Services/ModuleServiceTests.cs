using Moq;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Tests.Services;

public class ModuleServiceTests
{
    [Fact]
    public async Task GetByTrackAsync_ReturnsModulesInOrder()
    {
        var trackId = Guid.NewGuid();

        var modules = new List<Module>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Module 2",
                Summary = "Second module",
                Order = 2,
                TrackId = trackId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Module 1",
                Summary = "First module",
                Order = 1,
                TrackId = trackId
            }
        };

        var modulesMock = new Mock<IModuleRepository>();

        modulesMock
            .Setup(repository => repository.GetByTrackIdAsync(trackId))
            .ReturnsAsync(
                modules
                    .OrderBy(module => module.Order)
                    .ToList());

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.Modules)
            .Returns(modulesMock.Object);

        var service = new ModuleService(unitOfWorkMock.Object);
        var result = await service.GetByTrackAsync(trackId);

        Assert.Equal(2, result.Count);
        Assert.Equal("Module 1", result[0].Title);
        Assert.Equal("Module 2", result[1].Title);
        Assert.Equal(1, result[0].Order);
        Assert.Equal(2, result[1].Order);
    }

    [Fact]
    public async Task GetByIdAsync_WhenModuleExists_ReturnsModuleResponse()
    {
        var moduleId = Guid.NewGuid();
        var trackId = Guid.NewGuid();

        var module = new Module
        {
            Id = moduleId,
            Title = "C# Fundamentals",
            Summary = "Introduction to C#",
            Order = 1,
            TrackId = trackId
        };

        var modulesMock = new Mock<IModuleRepository>();

        modulesMock
            .Setup(repository => repository.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.Modules)
            .Returns(modulesMock.Object);

        var service = new ModuleService(unitOfWorkMock.Object);

        var result = await service.GetByIdAsync(moduleId);

        Assert.Equal(moduleId, result.Id);
        Assert.Equal("C# Fundamentals", result.Title);
        Assert.Equal("Introduction to C#", result.Summary);
        Assert.Equal(1, result.Order);
        Assert.Equal(trackId, result.TrackId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenModuleDoesNotExist_ThrowsNotFoundException()
    {
        var moduleId = Guid.NewGuid();

        var modulesMock = new Mock<IModuleRepository>();

        modulesMock
            .Setup(repository => repository.GetByIdAsync(moduleId))
            .ReturnsAsync((Module?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.Modules)
            .Returns(modulesMock.Object);

        var service = new ModuleService(unitOfWorkMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(moduleId));
    }
}
