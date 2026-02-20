using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic;

public class MessageLogic(IMessageRepository repository) : IMessageLogic
{
    private readonly IMessageRepository _repository = repository;

    public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
    {
        return await _repository.GetByIdAsync(organizationId, id);
    }

    public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
    {
        return await _repository.GetAllByOrganizationAsync(organizationId);
    }

    public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
    {
        var errors = ValidateMessage(request.Title, request.Content);
        if (errors.Count > 0)
        {
            return new ValidationError(errors);
        }

        var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (existing is not null)
        {
            return new Conflict($"A message with title '{request.Title}' already exists in this organization.");
        }

        var message = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content
        };

        var created = await _repository.CreateAsync(message);
        return new Created<Message>(created);
    }

    public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
    {
        var errors = ValidateMessage(request.Title, request.Content);
        if (errors.Count > 0)
        {
            return new ValidationError(errors);
        }

        var existingMessage = await _repository.GetByIdAsync(organizationId, id);
        if (existingMessage is null)
        {
            return new NotFound($"Message with id '{id}' was not found.");
        }

        if (!existingMessage.IsActive)
        {
            return new ValidationError(new Dictionary<string, string[]>
            {
                { "IsActive", new[] { "Cannot update an inactive message." } }
            });
        }

        var duplicateTitle = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (duplicateTitle is not null && duplicateTitle.Id != id)
        {
            return new Conflict($"A message with title '{request.Title}' already exists in this organization.");
        }

        existingMessage.Title = request.Title;
        existingMessage.Content = request.Content;
        existingMessage.IsActive = request.IsActive;

        await _repository.UpdateAsync(existingMessage);
        return new Updated();
    }

    public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
    {
        var existingMessage = await _repository.GetByIdAsync(organizationId, id);
        if (existingMessage is null)
        {
            return new NotFound($"Message with id '{id}' was not found.");
        }

        if (!existingMessage.IsActive)
        {
            return new ValidationError(new Dictionary<string, string[]>
            {
                { "IsActive", new[] { "Cannot delete an inactive message." } }
            });
        }

        await _repository.DeleteAsync(organizationId, id);
        return new Deleted();
    }

    private static Dictionary<string, string[]> ValidateMessage(string title, string content)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title) || title.Length < 3 || title.Length > 200)
        {
            errors["Title"] = new[] { "Title is required and must be between 3 and 200 characters." };
        }

        if (string.IsNullOrWhiteSpace(content) || content.Length < 10 || content.Length > 1000)
        {
            errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };
        }

        return errors;
    }
}
