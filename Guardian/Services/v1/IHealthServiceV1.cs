using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Services.V1;

public interface IHealthServiceV1
{
    Task<HealthResponse> GetHealthAsync();
}
