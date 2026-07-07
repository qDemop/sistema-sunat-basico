using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Features.Authentication;

namespace ERP.Application.Abstractions;

public interface IAuditWriter
{
    Task WriteAsync(AuditEventRecord record, CancellationToken cancellationToken = default);
}
