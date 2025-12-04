using System.Threading;
using System.Threading.Tasks;

namespace Guardian.Services.Common;

public interface IDatabaseService
{
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
