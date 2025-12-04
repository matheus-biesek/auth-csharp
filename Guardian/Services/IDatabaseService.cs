using System.Threading;
using System.Threading.Tasks;

namespace Guardian.Services;

public interface IDatabaseService
{
    // Test a simple connection to the database
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
