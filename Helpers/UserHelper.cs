using System.Security.Claims;
using System.Text.Json;
using Banko.Models;
using Banko.Services;

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

    public static string GenerateUserIdBase(string firstName, string lastName)
    {
      return $"@{firstName.ToLower()}{lastName.ToLower()}";
    }
    public static string GenerateUserIdVariant(string baseId, int counter = 0)
    {
      if (counter <= 0)
        return baseId;

      if (counter > 1000)
        return $"{baseId}{Guid.NewGuid().ToString()[..4]}";

      return $"{baseId}{counter}";
    }

    public static ServiceResult<DateTime?> ProcessAndValidateDateOfBirth(DateTime? dateOfBirthFromDto)
    {
      if (!dateOfBirthFromDto.HasValue)
      {
        // If no date is provided, it's not an error for processing,
        // it simply means no update to DateOfBirth. Return success with null.
        return ServiceResult<DateTime?>.Success(null);
      }

      DateTime dobFromDto = dateOfBirthFromDto.Value;

      // The model binder, when parsing a string, might produce a DateTime
      // with Kind=Unspecified. We need to explicitly state it's UTC for PostgreSQL.
      // For DateOfBirth, we typically care about the date part, and store it as midnight UTC on that day.
      DateTime dobToStoreUtc = new DateTime(dobFromDto.Year, dobFromDto.Month, dobFromDto.Day, 0, 0, 0, DateTimeKind.Utc);
      // Ensure the Kind is explicitly set (though the constructor above should do it)
      dobToStoreUtc = DateTime.SpecifyKind(dobToStoreUtc, DateTimeKind.Utc);


      if (dobToStoreUtc.Date < DateTime.UtcNow.Date)
      {
        return ServiceResult<DateTime?>.Success(dobToStoreUtc);
      }
      else
      {
        return ServiceResult<DateTime?>.Failure("Date of birth must be in the past.");
      }
    }
  }
}