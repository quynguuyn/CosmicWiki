using MediatR;

namespace CosmicWiki.Auth.Application.Features.Users.RemoveRole;

public sealed record RemoveRoleCommand(
    Guid UserId,
    Guid RoleId) : IRequest;