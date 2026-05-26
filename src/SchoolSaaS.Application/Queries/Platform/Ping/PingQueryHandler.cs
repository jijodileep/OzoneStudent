using MediatR;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Platform.Ping;

public sealed class PingQueryHandler : IRequestHandler<PingQuery, Result<string>>
{
    public Task<Result<string>> Handle(PingQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Result<string>.Success("pong"));
}

