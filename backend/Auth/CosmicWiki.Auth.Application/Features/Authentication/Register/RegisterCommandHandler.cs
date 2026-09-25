using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using CosmicWiki.Auth.Domain.ValueObjects;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
    }

    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var username = Username.Create(request.Username);
        var email = Email.Create(request.Email);

        if (string.Equals(username.Value, "admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The reserved administrator username cannot be registered.");
        }

        var usernameExists =
            await _userRepository.ExistsByUsernameAsync(
                username.Value,
                cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException(
                "Username already exists.");
        }

        var emailExists =
            await _userRepository.ExistsByEmailAsync(
                email.Value,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "Email already exists.");
        }

        var passwordHash =
            _passwordHasher.Hash(request.Password);

        var user = User.Create(
            username,
            email,
            passwordHash);

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        var userRole = await _roleRepository.GetByNameAsync(
            "User",
            cancellationToken)
            ?? throw new InvalidOperationException(
                "The default User role is not configured.");

        await _userRoleRepository.AddAsync(
            UserRole.Create(user.Id, userRole.Id),
            cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new RegisterResponse(
            user.Id,
            user.Username.Value,
            user.Email.Value);
    }
}