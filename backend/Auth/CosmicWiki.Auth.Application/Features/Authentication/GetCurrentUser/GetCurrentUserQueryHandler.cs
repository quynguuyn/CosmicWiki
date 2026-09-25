using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserQueryHandler(
        IUserRepository userRepository,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<GetCurrentUserResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Invalid access token.");
        }

        var user = await _userRepository.GetByIdAsync(
            _currentUser.UserId.Value,
            cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "User no longer exists.");
        }

        return new GetCurrentUserResponse(
            user.Id,
            user.Username.Value,
            user.Email.Value);
    }
}