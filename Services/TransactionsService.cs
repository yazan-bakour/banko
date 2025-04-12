using Banko.Data;
using Banko.Models;
using System.Text.Json;

// Adding async validation support
// Adding caching for frequently accessed accounts
// Including more specific validation messages
// Adding logging for validation failures
// add better class to get location.

namespace Banko.Services
{
  public class TransactionsService(AppDbContext context, IHttpContextAccessor HttpContext)
  {
    private readonly AppDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = HttpContext;
    private readonly HttpClient _httpClient = new HttpClient();
    public async Task<Transactions> CreateTransactionAsync(Transactions transaction)
    {
      string ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
      string userAgent = _httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString() ?? "Unknown";
      string location = await GetLocationAsync(ipAddress);

      Metadata metadata = new()
      {
        IpAddress = ipAddress,
        Device = userAgent,
        Location = location
      };

      transaction.Id = Guid.NewGuid();
      transaction.Status = TransactionStatus.Pending;
      transaction.CreatedAt = DateTime.UtcNow;
      transaction.UpdatedAt = DateTime.UtcNow;
      transaction.TransactionDate = DateTime.UtcNow;
      transaction.Currency = Currency.EUR;
      transaction.ReferenceNumber = $"REF-{Guid.NewGuid().ToString()[..8].ToUpper()}";
      transaction.SourceAccountId ??= string.Empty;
      transaction.Metadata = [metadata];
      transaction.UserId ??= string.Empty;
      transaction.PaymentMethod = PaymentMethod.CreditCard;

      _context.Transactions.Add(transaction);
      await _context.SaveChangesAsync();

      return transaction;
    }

    private async Task<string> GetLocationAsync(string ipAddress)
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