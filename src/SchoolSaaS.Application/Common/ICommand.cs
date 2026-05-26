using MediatR;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Common;

public interface ICommand<T> : IRequest<Result<T>>;
