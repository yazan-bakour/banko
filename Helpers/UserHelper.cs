using System.Security.Claims;
using System.Text.Json;

namespace Banko.Helpers
{
  public class UserHelper(IHttpContextAccessor HttpContext, ILogger<UserHelper> logger)
  {
    private readonly IHttpContextAccessor _httpContextAccessor = HttpContext;
    private readonly ILogger<UserHelper> _logger = logger;
    private readonly HttpClient _httpClient = new();

    public string GetCurrentSignedInUserId()
    {
      if (_httpContextAccessor.HttpContext == null)
      {
        _logger.LogWarning("HttpContext is null when attempting to get user ID");
        throw new InvalidOperationException("HttpContext is not available");
      }

      if (!_httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated ?? false)
      {
        _logger.LogWarning("Attempt to get user ID from unauthenticated request");
        throw new UnauthorizedAccessException("User is not authenticated");
      }

      var claims = _httpContextAccessor.HttpContext.User.Claims;

      string? userId = claims?.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier)?.Value;

      // Remove this log
      _logger.LogInformation("User ID: {UserId}", userId);

      if (string.IsNullOrEmpty(userId))
      {
        throw new UnauthorizedAccessException("User ID not found in token");
      }

      return userId;
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
  }
}