using Banko.Data;
using Banko.Models;
using Banko.Helpers;

// Adding async validation support
// Adding caching for frequently accessed accounts
// Including more specific validation messages
// Adding logging for validation failures
// add better class to get location.

namespace Banko.Services
{
  public class TransactionsService(UserHelper UserHelper, AppDbContext context, IHttpContextAccessor HttpContext, AccountService AccountService)
  {
    private readonly AppDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = HttpContext;
    private readonly AccountService _AccountService = AccountService;
    public async Task<Transactions> CreateTransactionAsync(Transactions transaction)
    {
      string userId = UserHelper.GetCurrentSignedInUserId();

      IEnumerable<Account> sourceUserAccounts = await _AccountService.GetAccountsByUserIdAsync(userId);
      Account sourceUserAccount = sourceUserAccounts.FirstOrDefault() ?? throw new UnauthorizedAccessException("No accounts found for user.");
      IEnumerable<Account> AllAccounts = await _AccountService.GetAllAccountsAsync();

      transaction.SourceAccountId = sourceUserAccount.Id.ToString();
      string? recipientAccountId = AllAccounts.FirstOrDefault(x => x.AccountNumber == transaction.AccountNumber)?.Id.ToString();

      if (transaction.AccountNumber == sourceUserAccount.AccountNumber)
      {
        throw new InvalidOperationException("You cannot transfer to your own account.");
      }
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
        Location = await UserHelper.GetLocationAsync(ipAddress)
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
  }
}