using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Services;

public class HealthServiceV1 : IHealthServiceV1
{
    public Task<HealthResponse> GetHealthAsync()
    {
        var res = new HealthResponse { Status = "Healthy", Version = "1.0" };
        return Task.FromResult(res);
    }
}
