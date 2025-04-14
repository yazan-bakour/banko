using System.Security.Claims;
using System.Text.Json;

// split into specific helper classes
// register the helper in the DependencyInjection

namespace Banko.Helper
{
  public class Helper(IHttpContextAccessor HttpContext)
  {
    private readonly IHttpContextAccessor _httpContextAccessor = HttpContext;
    private readonly HttpClient _httpClient = new();
    public static string GenerateAccountNumber()
    {
      Random random = new();
      return "NKO" + random.Next(10000000, 99999999).ToString();
    }

    public async Task<string> GetLocationAsync(string ipAddress)
    {
      try
      {
        var response = await _httpClient.GetStringAsync($"https://some-ip-api.com/{ipAddress}");

        var locationData = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

        if (locationData == null)
        {
          return "Unknown location";
        }

        string? city = locationData.TryGetValue("city", out var cityValue) ? cityValue?.ToString() : "Unknown";
        string? country = locationData.TryGetValue("country", out var countryValue) ? countryValue?.ToString() : "Unknown";

        return $"{city}, {country}";
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error getting location: {ex.Message}");
        return "Unknown location";
      }
    }

    public string GetCurrentSignedInUserId()
    {
      var claims = _httpContextAccessor.HttpContext?.User?.Claims;
      string? userId = claims?.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier)?.Value;
      if (string.IsNullOrEmpty(userId))
      {
        throw new UnauthorizedAccessException("User ID not found in token");
      }

      return userId;
    }

    // public string GenerateJwtToken(User user)
  }
}