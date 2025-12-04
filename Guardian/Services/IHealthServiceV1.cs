using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Services;

public interface IHealthServiceV1
{
    Task<HealthResponse> GetHealthAsync();
}
