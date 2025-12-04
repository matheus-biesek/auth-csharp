using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Services.V2;

public class HealthServiceV2 : IHealthServiceV2
{
    private readonly Guardian.Services.Common.IDatabaseService _db;

    public HealthServiceV2(Guardian.Services.Common.IDatabaseService db)
    {
        _db = db;
    }

    public async Task<HealthResponse> GetHealthAsync()
    {
        var dbOk = await _db.TestConnectionAsync();
        var details = new { message = "v2 endpoint", db = dbOk };
        var res = new HealthResponse { Status = "Healthy", Version = "2.0", Details = details };
        return res;
    }
}
