using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Services.V2;

public interface IHealthServiceV2
{
    Task<HealthResponse> GetHealthAsync();
}
