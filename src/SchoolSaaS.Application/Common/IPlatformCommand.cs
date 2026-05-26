using MediatR;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Common;

/// <summary>
/// Marker for platform-level commands (tenant registry). Does not require the current request tenant scope.
/// </summary>
public interface IPlatformCommand;

/// <summary>
/// Platform-level command with a typed result.
/// </summary>
public interface IPlatformCommand<T> : IPlatformCommand, IRequest<Result<T>>;
