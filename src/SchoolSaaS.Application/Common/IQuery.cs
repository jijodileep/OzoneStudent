using MediatR;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Common;

public interface IQuery<T> : IRequest<Result<T>>;
