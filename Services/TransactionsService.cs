using Banko.Data;
using Banko.Models;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// Adding async validation support
// Adding caching for frequently accessed accounts
// Including more specific validation messages
// Adding logging for validation failures
// add better class to get location.

namespace Banko.Services
{
  public class TransactionsService(AppDbContext context, IHttpContextAccessor HttpContext, AccountService AccountService)
  {
    private readonly AppDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = HttpContext;
    private readonly HttpClient _httpClient = new();
    private readonly AccountService _AccountService = AccountService;
    public async Task<Transactions> CreateTransactionAsync(Transactions transaction)
    {
      string userId = GetCurrentSignedInUserId();

      IEnumerable<Account> sourceUserAccounts = await _AccountService.GetAccountsByUserIdAsync(userId);
      Account sourceUserAccount = sourceUserAccounts.FirstOrDefault() ?? throw new UnauthorizedAccessException("No accounts found for user.");
      IEnumerable<Account> AllAccounts = await _AccountService.GetAllAccountsAsync();

      transaction.SourceAccountId = sourceUserAccount.Id.ToString();
      string? recipientAccountId = AllAccounts.FirstOrDefault(x => x.AccountNumber == transaction.AccountNumber)?.Id.ToString();

      if (sourceUserAccount.Balance < transaction.Amount)
      {
        throw new InvalidOperationException("Insufficient funds.");
      }

      string ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

      if (sourceUserAccount?.Id.ToString() != transaction.SourceAccountId)
      {
        throw new UnauthorizedAccessException("Please sign in to make a transaction.");
      }

      Metadata metadata = new()
      {
        IpAddress = ipAddress,
        Device = _httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString() ?? "Unknown",
        Location = await GetLocationAsync(ipAddress)
      };

      transaction.Id = Guid.NewGuid();
      transaction.Status = TransactionStatus.Created;
      transaction.CreatedAt = DateTime.UtcNow;
      transaction.UpdatedAt = DateTime.UtcNow;
      transaction.TransactionDate = DateTime.UtcNow;
      transaction.Currency = Currency.EUR;
      transaction.ReferenceNumber = $"REF-{Guid.NewGuid().ToString()[..8].ToUpper()}";
      transaction.DestinationAccountId = recipientAccountId;
      transaction.Metadata = [metadata];
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
  }
}