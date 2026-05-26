namespace SchoolSaaS.Application.Abstractions.Auth;

public interface IRefreshTokenService
{
    Task<RefreshTokenIssueResult> IssueAsync(
        Guid userId,
        Guid tenantId,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenValidationResult> ValidateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed record RefreshTokenIssueResult(string Token, DateTime ExpiresAt);

public sealed record RefreshTokenValidationResult(
    bool IsValid,
    Guid? UserId = null,
    Guid? TenantId = null,
    string? FailureReason = null);
