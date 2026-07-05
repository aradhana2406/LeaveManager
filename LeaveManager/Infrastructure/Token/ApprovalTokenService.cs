using System.Text;

namespace LeaveManager.Infrastructure.Token;

public interface IApprovalTokenService
{
    string GenerateToken(int leaveApplicationId, int approverId);
    (int LeaveApplicationId, int ApproverId) DecodeToken(string token);
}

public class ApprovalTokenService : IApprovalTokenService
{
    private readonly IConfiguration _configuration;
    private const string TokenKey = "ApprovalToken";

    public ApprovalTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(int leaveApplicationId, int approverId)
    {
        // Format: leaveAppId|approverId|timestamp|checksum
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = $"{leaveApplicationId}|{approverId}|{timestamp}";

        // Simple encoding (in production, use JWT)
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        return encoded.Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public (int LeaveApplicationId, int ApproverId) DecodeToken(string token)
    {
        try
        {
            // Reverse the encoding
            var padding = 4 - (token.Length % 4);
            if (padding != 4) token += new string('=', padding);

            token = token.Replace("-", "+").Replace("_", "/");
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));

            var parts = decoded.Split('|');
            if (parts.Length != 3)
                throw new InvalidOperationException("Invalid token format");

            var leaveAppId = int.Parse(parts[0]);
            var approverId = int.Parse(parts[1]);
            var timestamp = long.Parse(parts[2]);

            // Check if token is not older than 7 days
            var tokenAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
            if (tokenAge > 7 * 24 * 60 * 60) // 7 days
                throw new InvalidOperationException("Token expired");

            return (leaveAppId, approverId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to decode token: {ex.Message}");
        }
    }
}