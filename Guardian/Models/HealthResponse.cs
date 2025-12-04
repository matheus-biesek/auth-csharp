namespace Guardian.Models;

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public object? Details { get; set; }
}
