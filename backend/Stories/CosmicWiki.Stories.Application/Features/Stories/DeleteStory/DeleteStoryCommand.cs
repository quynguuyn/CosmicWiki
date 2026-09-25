using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.DeleteStory;

public sealed record DeleteStoryCommand(Guid Id) : IRequest;
