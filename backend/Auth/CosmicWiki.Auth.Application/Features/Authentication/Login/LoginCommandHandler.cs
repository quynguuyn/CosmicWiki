using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Enums;
using CosmicWiki.Auth.Domain.ValueObjects;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.Login;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserPermissionRepository _userPermissionRepository;
    private readonly IRefreshTokenService _refreshTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUserPermissionRepository userPermissionRepository,
        IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _userPermissionRepository = userPermissionRepository;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var username = Username.Create(request.Username);

        var user = await _userRepository.GetByUsernameAsync(username.Value, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }
        
        var permissions = await _userPermissionRepository.GetPermissionCodesAsync(user.Id, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user, permissions);
        var refreshToken = await _refreshTokenService.CreateAsync(
            user.Id,
            request.RememberMe,
            cancellationToken);

        return new LoginResponse(
            accessToken,
            3600,
            refreshToken.RefreshToken,
            refreshToken.ExpiresAtUtc,
            refreshToken.RememberMe);
    }
}