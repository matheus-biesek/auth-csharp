using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Services.V1;

public class HealthServiceV1 : IHealthServiceV1
{
    private readonly Guardian.Services.Common.IDatabaseService _db;

    public HealthServiceV1(Guardian.Services.Common.IDatabaseService db)
    {
        _db = db;
    }

    public async Task<HealthResponse> GetHealthAsync()
    {
        var dbOk = await _db.TestConnectionAsync();
        var details = new { message = "v1 endpoint", db = dbOk };
        var res = new HealthResponse { Status = "Healthy", Version = "1.0", Details = details };
        return res;
    }
}
