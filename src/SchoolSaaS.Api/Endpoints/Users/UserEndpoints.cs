using MediatR;
using SchoolSaaS.Application.Commands.Users.InviteUser;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Users;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapPost("/invite", async (InviteUserRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<InviteUserResult> result = await mediator.Send(
                new InviteUserCommand(body.Email, body.RoleName, body.FirstName, body.LastName),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/users/invitations/{result.Value!.InvitationId}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                    ? Results.Conflict(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("InviteUser")
        .Produces<InviteUserResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private sealed record InviteUserRequest(
        string Email,
        string RoleName,
        string? FirstName = null,
        string? LastName = null);
}
