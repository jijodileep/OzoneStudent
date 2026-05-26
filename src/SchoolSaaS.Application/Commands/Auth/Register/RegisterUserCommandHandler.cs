using MediatR;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Auth.Register;

public sealed class RegisterUserCommandHandler(
    ITenantContext tenantContext,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterUserCommand, Result<RegisterUserResult>>
{
    public async Task<Result<RegisterUserResult>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<RegisterUserResult>.Failure(
                "auth.tenant_required",
                "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(tenantId, normalizedEmail, cancellationToken))
        {
            return Result<RegisterUserResult>.Conflict("A user with this email already exists.");
        }

        var role = await roleRepository.FindByNameAsync(tenantId, request.RoleName, cancellationToken);
        if (role is null)
        {
            return Result<RegisterUserResult>.Failure(
                "roles.not_found",
                $"Role '{request.RoleName}' was not found.");
        }

        var user = new User
        {
            TenantId = tenantId,
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            Status = UserStatus.Active,
            EmailConfirmed = true,
            CreatedBy = tenantContext.UserId
        };

        var profile = new UserProfile
        {
            TenantId = tenantId,
            UserId = user.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedBy = tenantContext.UserId
        };

        await userRepository.AddAsync(user, profile, cancellationToken);
        await roleRepository.AssignRoleToUserAsync(
            tenantId,
            user.Id,
            role.Id,
            tenantContext.UserId,
            cancellationToken);

        return Result<RegisterUserResult>.Success(new RegisterUserResult(user.Id, user.Email));
    }
}

