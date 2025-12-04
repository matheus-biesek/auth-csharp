using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Services;

public interface IHealthServiceV2
{
    Task<HealthResponse> GetHealthAsync();
}
