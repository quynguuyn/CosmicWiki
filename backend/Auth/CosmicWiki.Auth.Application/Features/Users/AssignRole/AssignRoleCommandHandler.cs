using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.Users.AssignRole;

public sealed class AssignRoleCommandHandler
    : IRequestHandler<AssignRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        AssignRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
            throw new KeyNotFoundException(
                $"User '{request.UserId}' was not found.");

        var role = await _roleRepository.GetByIdAsync(
            request.RoleId,
            cancellationToken);

        if (role is null)
            throw new KeyNotFoundException(
                $"Role '{request.RoleId}' was not found.");

        var exists = await _userRoleRepository.ExistsAsync(
            request.UserId,
            request.RoleId,
            cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                $"Role '{role.Name}' is already assigned to user '{user.Username.Value}'.");

        var userRole = UserRole.Create(
            request.UserId,
            request.RoleId);

        await _userRoleRepository.AddAsync(
            userRole,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}