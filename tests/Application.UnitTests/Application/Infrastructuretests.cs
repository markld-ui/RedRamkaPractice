using API.Services;
using Application.Features.Projects.Commands.Shared;
using Application.Features.Projects.Events;
using Domain.Events;
using Domain.Projects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.UnitTests.Application;

/// <summary>
/// Тесты обработчика события <see cref="ProjectCreatedEventHandler"/>.
/// </summary>
public class ProjectCreatedEventHandlerTests
{
    private readonly Mock<ILogger<ProjectCreatedEventHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldCompleteWithoutException()
    {
        var handler = new ProjectCreatedEventHandler(_loggerMock.Object);
        var project = new Project("Test Project", "Description");
        var notification = new ProjectCreatedEvent(project);

        var act = async () => await handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        var handler = new ProjectCreatedEventHandler(_loggerMock.Object);
        var project = new Project("My Project", "Description");
        var notification = new ProjectCreatedEvent(project);

        await handler.Handle(notification, CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains(project.Id.ToString()) &&
                    v.ToString()!.Contains("My Project")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCompletedTask()
    {
        var handler = new ProjectCreatedEventHandler(_loggerMock.Object);
        var project = new Project("P", "D");
        var notification = new ProjectCreatedEvent(project);

        var task = handler.Handle(notification, CancellationToken.None);

        await task;
        task.IsCompleted.Should().BeTrue();
    }
}

/// <summary>
/// Тесты сервиса <see cref="DateTimeService"/>.
/// </summary>
public class DateTimeServiceTests
{
    [Fact]
    public void Now_ShouldReturnCurrentUtcTime()
    {
        var service = new DateTimeService();
        var before = DateTime.UtcNow;

        var result = service.Now;

        var after = DateTime.UtcNow;

        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Now_ShouldReturnUtcKind()
    {
        var service = new DateTimeService();

        var result = service.Now;

        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Now_CalledTwice_SecondShouldBeOnOrAfterFirst()
    {
        var service = new DateTimeService();

        var first = service.Now;
        var second = service.Now;

        second.Should().BeOnOrAfter(first);
    }
}

/// <summary>
/// Тесты DTO <see cref="TransitionResult"/> уровня Application.
/// </summary>
public class TransitionResultTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var result = new TransitionResult(true, null, "Development");

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.NewStage.Should().Be("Development");
    }

    [Fact]
    public void FailResult_ShouldHaveCorrectValues()
    {
        var result = new TransitionResult(false, "Cannot transition from current stage.", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Cannot transition from current stage.");
        result.NewStage.Should().BeNull();
    }

    [Fact]
    public void Record_Equality_ShouldWorkCorrectly()
    {
        var r1 = new TransitionResult(true, null, "QA");
        var r2 = new TransitionResult(true, null, "QA");

        r1.Should().Be(r2);
    }

    [Fact]
    public void Record_Inequality_WhenStagesDiffer_ShouldNotBeEqual()
    {
        var r1 = new TransitionResult(true, null, "QA");
        var r2 = new TransitionResult(true, null, "Development");

        r1.Should().NotBe(r2);
    }

    [Theory]
    [InlineData("Design")]
    [InlineData("Development")]
    [InlineData("QA")]
    [InlineData("Delivery")]
    [InlineData("Support")]
    [InlineData("Archived")]
    public void NewStage_ShouldAcceptAllProjectStageNames(string stageName)
    {
        var result = new TransitionResult(true, null, stageName);

        result.NewStage.Should().Be(stageName);
    }
}