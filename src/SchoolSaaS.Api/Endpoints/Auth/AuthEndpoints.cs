using MediatR;
using SchoolSaaS.Application.Commands.Auth.AcceptInvitation;
using SchoolSaaS.Application.Commands.Auth.ChangePassword;
using SchoolSaaS.Application.Commands.Auth.ForgotPassword;
using SchoolSaaS.Application.Commands.Auth.Login;
using SchoolSaaS.Application.Commands.Auth.Logout;
using SchoolSaaS.Application.Commands.Auth.RefreshToken;
using SchoolSaaS.Application.Commands.Auth.Register;
using SchoolSaaS.Application.Commands.Auth.ResetPassword;
using SchoolSaaS.Application.Queries.Auth.GetCurrentUser;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/login", async (LoginRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<LoginResult> result = await mediator.Send(new LoginCommand(body.Email, body.Password), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Json(new { errors = result.Errors }, statusCode: StatusCodes.Status401Unauthorized);
        })
        .WithName("Login")
        .AllowAnonymous()
        .Produces<LoginResult>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", async (RefreshRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<LoginResult> result = await mediator.Send(new RefreshTokenCommand(body.RefreshToken), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Json(new { errors = result.Errors }, statusCode: StatusCodes.Status401Unauthorized);
        })
        .WithName("RefreshToken")
        .AllowAnonymous()
        .Produces<LoginResult>();

        group.MapPost("/logout", async (RefreshRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<bool> result = await mediator.Send(new LogoutCommand(body.RefreshToken), ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
        })
        .WithName("Logout")
        .AllowAnonymous();

        group.MapPost("/register", async (RegisterRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<RegisterUserResult> result = await mediator.Send(
                new RegisterUserCommand(body.Email, body.Password, body.FirstName, body.LastName, body.RoleName ?? "tenant_admin"),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/auth/me", result.Value)
                : Results.BadRequest(result.Errors);
        })
        .WithName("RegisterUser")
        .RequireAuthorization()
        .Produces<RegisterUserResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/me", async (IMediator mediator, CancellationToken ct) =>
        {
            Result<CurrentUserDto> result = await mediator.Send(new GetCurrentUserQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.Json(new { errors = result.Errors }, statusCode: StatusCodes.Status401Unauthorized);
        })
        .WithName("GetCurrentUser")
        .RequireAuthorization()
        .Produces<CurrentUserDto>();

        group.MapPost("/forgot-password", async (ForgotPasswordRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<ForgotPasswordResult> result = await mediator.Send(new ForgotPasswordCommand(body.Email), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("ForgotPassword")
        .AllowAnonymous()
        .Produces<ForgotPasswordResult>();

        group.MapPost("/reset-password", async (ResetPasswordRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<bool> result = await mediator.Send(new ResetPasswordCommand(body.Token, body.NewPassword), ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
        })
        .WithName("ResetPassword")
        .AllowAnonymous();

        group.MapPut("/me/password", async (ChangePasswordRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<bool> result = await mediator.Send(
                new ChangePasswordCommand(body.CurrentPassword, body.NewPassword),
                ct);

            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
        })
        .WithName("ChangePassword")
        .RequireAuthorization();

        group.MapPost("/accept-invitation", async (AcceptInvitationRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<AcceptInvitationResult> result = await mediator.Send(
                new AcceptInvitationCommand(body.Token, body.Password),
                ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                    ? Results.Conflict(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("AcceptInvitation")
        .AllowAnonymous()
        .Produces<AcceptInvitationResult>();

        return group;
    }

    private sealed record LoginRequest(string Email, string Password);

    private sealed record RefreshRequest(string RefreshToken);

    private sealed record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string? RoleName);

    private sealed record ForgotPasswordRequest(string Email);

    private sealed record ResetPasswordRequest(string Token, string NewPassword);

    private sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    private sealed record AcceptInvitationRequest(string Token, string Password);
}
