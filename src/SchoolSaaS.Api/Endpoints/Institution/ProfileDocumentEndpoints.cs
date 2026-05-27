using MediatR;
using SchoolSaaS.Application.Commands.Institution.DeleteProfileDocument;
using SchoolSaaS.Application.Commands.Institution.UploadProfileDocument;
using SchoolSaaS.Application.Queries.Institution.DownloadProfileDocument;
using SchoolSaaS.Application.Queries.Institution.ListProfileDocuments;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Institution;

public static class ProfileDocumentEndpoints
{
    public static RouteGroupBuilder MapStaffDocumentEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/staff/{staffId:guid}/documents")
            .WithTags("Institution")
            .RequireAuthorization();

        group.MapGet("/", async (Guid staffId, IMediator mediator, CancellationToken ct) =>
        {
            Result<ListProfileDocumentsResult> result = await mediator.Send(new ListStaffDocumentsQuery(staffId), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("ListStaffDocuments")
        .Produces<ListProfileDocumentsResult>();

        group.MapPost("/", async (
            Guid staffId,
            HttpRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { errors = new[] { new { code = "documents.invalid_form", message = "Multipart form data is required." } } });
            }

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { errors = new[] { new { code = "documents.file_required", message = "File is required." } } });
            }

            var documentType = form["documentType"].ToString();
            if (string.IsNullOrWhiteSpace(documentType))
            {
                documentType = "general";
            }

            await using Stream stream = file.OpenReadStream();
            Result<ProfileDocumentDto> result = await mediator.Send(
                new UploadStaffDocumentCommand(
                    staffId,
                    documentType,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    stream),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/staff/{staffId}/documents/{result.Value!.Id}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .DisableAntiforgery()
        .WithName("UploadStaffDocument")
        .Produces<ProfileDocumentDto>(StatusCodes.Status201Created);

        group.MapGet("/{documentId:guid}/download", async (
            Guid staffId,
            Guid documentId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<Application.Institution.ProfileDocumentDownloadResult> result = await mediator.Send(
                new DownloadStaffDocumentQuery(staffId, documentId),
                ct);

            if (!result.IsSuccess)
            {
                return result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
            }

            return Results.File(result.Value!.Content, result.Value.MimeType, result.Value.FileName);
        })
        .WithName("DownloadStaffDocument");

        group.MapDelete("/{documentId:guid}", async (
            Guid staffId,
            Guid documentId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<bool> result = await mediator.Send(new DeleteStaffDocumentCommand(staffId, documentId), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("DeleteStaffDocument");

        return group;
    }

    public static RouteGroupBuilder MapStudentDocumentEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/students/{studentId:guid}/documents")
            .WithTags("Students")
            .RequireAuthorization();

        group.MapGet("/", async (Guid studentId, IMediator mediator, CancellationToken ct) =>
        {
            Result<ListProfileDocumentsResult> result = await mediator.Send(new ListStudentDocumentsQuery(studentId), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("ListStudentDocuments")
        .Produces<ListProfileDocumentsResult>();

        group.MapPost("/", async (
            Guid studentId,
            HttpRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { errors = new[] { new { code = "documents.invalid_form", message = "Multipart form data is required." } } });
            }

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { errors = new[] { new { code = "documents.file_required", message = "File is required." } } });
            }

            var documentType = form["documentType"].ToString();
            if (string.IsNullOrWhiteSpace(documentType))
            {
                documentType = "general";
            }

            await using Stream stream = file.OpenReadStream();
            Result<ProfileDocumentDto> result = await mediator.Send(
                new UploadStudentDocumentCommand(
                    studentId,
                    documentType,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    stream),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/students/{studentId}/documents/{result.Value!.Id}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .DisableAntiforgery()
        .WithName("UploadStudentDocument")
        .Produces<ProfileDocumentDto>(StatusCodes.Status201Created);

        group.MapGet("/{documentId:guid}/download", async (
            Guid studentId,
            Guid documentId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<Application.Institution.ProfileDocumentDownloadResult> result = await mediator.Send(
                new DownloadStudentDocumentQuery(studentId, documentId),
                ct);

            if (!result.IsSuccess)
            {
                return result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
            }

            return Results.File(result.Value!.Content, result.Value.MimeType, result.Value.FileName);
        })
        .WithName("DownloadStudentDocument");

        group.MapDelete("/{documentId:guid}", async (
            Guid studentId,
            Guid documentId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<bool> result = await mediator.Send(new DeleteStudentDocumentCommand(studentId, documentId), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("DeleteStudentDocument");

        return group;
    }
}
