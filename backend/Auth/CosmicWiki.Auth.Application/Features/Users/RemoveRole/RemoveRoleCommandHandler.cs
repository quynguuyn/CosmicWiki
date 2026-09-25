using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.Users.RemoveRole;

public sealed class RemoveRoleCommandHandler
    : IRequestHandler<RemoveRoleCommand>
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleCommandHandler(
        IUserRoleRepository userRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemoveRoleCommand request,
        CancellationToken cancellationToken)
    {
        var userRole = await _userRoleRepository.GetAsync(
            request.UserId,
            request.RoleId,
            cancellationToken);

        if (userRole is null)
            throw new KeyNotFoundException(
                $"Role '{request.RoleId}' is not assigned to user '{request.UserId}'.");

        _userRoleRepository.Delete(userRole);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}