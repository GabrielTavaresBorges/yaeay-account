using MediatR;
using YaeaY.Account.Application.Services.Administration.Interfaces;
namespace YaeaY.Account.Application.UseCases.Administration.Queries.GetAudit;
public sealed class Handler(IAdministrationReader reader) : IRequestHandler<Query, IReadOnlyList<AuditEntry>> { public Task<IReadOnlyList<AuditEntry>> Handle(Query request, CancellationToken cancellationToken) => reader.GetAuditAsync(cancellationToken); }
