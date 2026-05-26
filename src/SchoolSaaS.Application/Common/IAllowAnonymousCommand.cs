namespace SchoolSaaS.Application.Common;

/// <summary>
/// Marker for commands that intentionally skip <see cref="Shared.Authorization.RequirePermissionAttribute"/>.
/// </summary>
public interface IAllowAnonymousCommand;
