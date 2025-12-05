namespace Guardian.Models.Auth.v1;

public class LoginResponse
{
    // Tokens are delivered via secure cookies; do not include sensitive tokens in response body.
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}
