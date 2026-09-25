using MediatR;

namespace CosmicWiki.Auth.Application.Features.Users.AssignRole;

public sealed record AssignRoleCommand(
    Guid UserId,
    Guid RoleId) : IRequest;