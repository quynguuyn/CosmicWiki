using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetAccounts;

public sealed class GetAdminAccountsQueryHandler(
    IAdminAccountRepository repository)
    : IRequestHandler<GetAdminAccountsQuery, IReadOnlyCollection<AdminAccountResponse>>
{
    public async Task<IReadOnlyCollection<AdminAccountResponse>> Handle(
        GetAdminAccountsQuery request,
        CancellationToken cancellationToken) =>
        (await repository.GetAccountsAsync(request.Role, cancellationToken))
        .Select(x => new AdminAccountResponse(
            x.UserId, x.Username, x.Email, x.Status, x.CreatedAt, x.Roles))
        .ToArray();
}
