using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using FluentAssertions;
using Moq;

namespace CodeChallenge.Tests;

public class MessageLogicTests
{
    private readonly Mock<IMessageRepository> _mockRepository;
    private readonly MessageLogic _logic;
    private readonly Guid _organizationId = Guid.NewGuid();

    public MessageLogicTests()
    {
        _mockRepository = new Mock<IMessageRepository>();
        _logic = new MessageLogic(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateMessageAsync_WithValidRequest_ReturnsCreated()
    {
        var request = new CreateMessageRequest
        {
            Title = "Valid Title",
            Content = "This is valid content with enough characters."
        };

        _mockRepository
            .Setup(r => r.GetByTitleAsync(_organizationId, request.Title))
            .ReturnsAsync((Message?)null);

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Message>()))
            .ReturnsAsync((Message m) =>
            {
                m.Id = Guid.NewGuid();
                m.CreatedAt = DateTime.UtcNow;
                return m;
            });

        var result = await _logic.CreateMessageAsync(_organizationId, request);

        result.Should().BeOfType<Created<Message>>();
        var created = (Created<Message>)result;
        created.Value.Title.Should().Be(request.Title);
        created.Value.Content.Should().Be(request.Content);
        created.Value.OrganizationId.Should().Be(_organizationId);
    }

    [Fact]
    public async Task CreateMessageAsync_WithDuplicateTitle_ReturnsConflict()
    {
        var request = new CreateMessageRequest
        {
            Title = "Duplicate Title",
            Content = "This is valid content with enough characters."
        };

        _mockRepository
            .Setup(r => r.GetByTitleAsync(_organizationId, request.Title))
            .ReturnsAsync(new Message { Title = request.Title });

        var result = await _logic.CreateMessageAsync(_organizationId, request);

        result.Should().BeOfType<Conflict>();
        var conflict = (Conflict)result;
        conflict.Message.Should().Contain(request.Title);
    }

    [Fact]
    public async Task CreateMessageAsync_WithInvalidContentLength_ReturnsValidationError()
    {
        var request = new CreateMessageRequest
        {
            Title = "Valid Title",
            Content = "Short"
        };

        var result = await _logic.CreateMessageAsync(_organizationId, request);

        result.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)result;
        validation.Errors.Should().ContainKey("Content");
    }

    [Fact]
    public async Task UpdateMessageAsync_WithNonExistentMessage_ReturnsNotFound()
    {
        var messageId = Guid.NewGuid();
        var request = new UpdateMessageRequest
        {
            Title = "Updated Title",
            Content = "This is valid updated content with enough characters.",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(_organizationId, messageId))
            .ReturnsAsync((Message?)null);

        var result = await _logic.UpdateMessageAsync(_organizationId, messageId, request);

        result.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task UpdateMessageAsync_WithInactiveMessage_ReturnsValidationError()
    {
        var messageId = Guid.NewGuid();
        var request = new UpdateMessageRequest
        {
            Title = "Updated Title",
            Content = "This is valid updated content with enough characters.",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(_organizationId, messageId))
            .ReturnsAsync(new Message
            {
                Id = messageId,
                OrganizationId = _organizationId,
                IsActive = false
            });

        var result = await _logic.UpdateMessageAsync(_organizationId, messageId, request);

        result.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)result;
        validation.Errors.Should().ContainKey("IsActive");
    }

    [Fact]
    public async Task DeleteMessageAsync_WithNonExistentMessage_ReturnsNotFound()
    {
        var messageId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdAsync(_organizationId, messageId))
            .ReturnsAsync((Message?)null);

        var result = await _logic.DeleteMessageAsync(_organizationId, messageId);

        result.Should().BeOfType<NotFound>();
    }
}
