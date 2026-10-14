using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetAccounts;

public sealed record GetAdminAccountsQuery(string? Role)
    : IRequest<IReadOnlyCollection<AdminAccountResponse>>;

public sealed record AdminAccountResponse(
    Guid UserId,
    string Username,
    string Email,
    string Status,
    DateTime CreatedAt,
    IReadOnlyCollection<string> Roles);
