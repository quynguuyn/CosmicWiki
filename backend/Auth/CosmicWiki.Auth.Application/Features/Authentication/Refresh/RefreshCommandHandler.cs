using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Enums;
using CosmicWiki.Auth.Application.Features.Authentication.Login;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.Refresh;

public sealed class RefreshCommandHandler
    : IRequestHandler<RefreshCommand, LoginResponse>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IUserPermissionRepository _userPermissionRepository;
    private readonly ITokenService _tokenService;

    public RefreshCommandHandler(
        IRefreshTokenService refreshTokenService,
        IUserRepository userRepository,
        IUserPermissionRepository userPermissionRepository,
        ITokenService tokenService)
    {
        _refreshTokenService = refreshTokenService;
        _userRepository = userRepository;
        _userPermissionRepository = userPermissionRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> Handle(
        RefreshCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedAccessException(
                "A valid refresh token is required.");
        }

        var rotation = await _refreshTokenService.RotateAsync(
            request.RefreshToken,
            cancellationToken);

        if (rotation is null)
        {
            throw new UnauthorizedAccessException(
                "The refresh token is invalid or expired.");
        }

        var user = await _userRepository.GetByIdAsync(
            rotation.UserId,
            cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException(
                "The refresh token is invalid or expired.");
        }

        var permissions = await _userPermissionRepository.GetPermissionCodesAsync(
            user.Id,
            cancellationToken);

        return new LoginResponse(
            _tokenService.GenerateAccessToken(user, permissions),
            3600,
            rotation.RefreshToken,
            rotation.ExpiresAtUtc,
            rotation.RememberMe);
    }
}
