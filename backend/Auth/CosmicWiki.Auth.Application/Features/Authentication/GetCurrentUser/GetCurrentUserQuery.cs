using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.GetCurrentUser;

public sealed record GetCurrentUserQuery
    : IRequest<GetCurrentUserResponse>;