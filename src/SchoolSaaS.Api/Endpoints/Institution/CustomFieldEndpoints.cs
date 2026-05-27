using MediatR;
using SchoolSaaS.Application.Commands.Institution.CreateCustomFieldDefinition;
using SchoolSaaS.Application.Commands.Institution.UpdateCustomFieldDefinition;
using SchoolSaaS.Application.Queries.Institution.ListCustomFieldDefinitions;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Institution;

public static class CustomFieldEndpoints
{
    public static RouteGroupBuilder MapCustomFieldEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/custom-fields")
            .WithTags("Institution")
            .RequireAuthorization();

        group.MapGet("/", async (
            CustomFieldEntityType entityType,
            bool? activeOnly,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<ListCustomFieldDefinitionsResult> result = await mediator.Send(
                new ListCustomFieldDefinitionsQuery(entityType, activeOnly ?? true),
                ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("ListCustomFieldDefinitions")
        .Produces<ListCustomFieldDefinitionsResult>();

        group.MapPost("/", async (CreateCustomFieldDefinitionRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<CustomFieldDefinitionDto> result = await mediator.Send(
                new CreateCustomFieldDefinitionCommand(
                    body.EntityType,
                    body.FieldKey,
                    body.Label,
                    body.DataType,
                    body.Options,
                    body.IsRequired ?? false,
                    body.SortOrder ?? 0),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/custom-fields/{result.Value!.Id}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                    ? Results.Conflict(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("CreateCustomFieldDefinition")
        .Produces<CustomFieldDefinitionDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCustomFieldDefinitionRequest body,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<CustomFieldDefinitionDto> result = await mediator.Send(
                new UpdateCustomFieldDefinitionCommand(
                    id,
                    body.Label,
                    body.IsRequired,
                    body.SortOrder,
                    body.IsActive,
                    body.Options),
                ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("UpdateCustomFieldDefinition")
        .Produces<CustomFieldDefinitionDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private sealed record CreateCustomFieldDefinitionRequest(
        CustomFieldEntityType EntityType,
        string FieldKey,
        string Label,
        CustomFieldDataType DataType,
        IReadOnlyList<string>? Options = null,
        bool? IsRequired = null,
        int? SortOrder = null);

    private sealed record UpdateCustomFieldDefinitionRequest(
        string Label,
        bool IsRequired,
        int SortOrder,
        bool IsActive,
        IReadOnlyList<string>? Options = null);
}
