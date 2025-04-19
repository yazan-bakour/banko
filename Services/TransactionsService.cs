using Banko.Data;
using Banko.Models;
using Banko.Helpers;
using Microsoft.EntityFrameworkCore;

// Adding async validation support
// Adding caching for frequently accessed accounts
// Including more specific validation messages
// Adding logging for validation failures
// add better class to get location or check if the location is not enabled

namespace Banko.Services
{
  public class TransactionsService(UserHelper UserHelper, AppDbContext context, IHttpContextAccessor HttpContext, AccountService AccountService)
  {
    private readonly AppDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = HttpContext;
    private readonly AccountService _AccountService = AccountService;

    public async Task<IEnumerable<Transactions>> GetAllTransactionsByUserIdAsync()
    {
      string userId = UserHelper.GetCurrentSignedInUserId();

      IEnumerable<Account> sourceUserAccounts = await _AccountService.GetAccountsByUserIdAsync(userId);
      Account sourceUserAccount = sourceUserAccounts.FirstOrDefault() ?? throw new UnauthorizedAccessException("No accounts found for user.");

      return await _context.Transactions
        .Where(x => x.SourceAccountId == sourceUserAccount.Id.ToString())
        .ToListAsync();
    }

    public async Task<IEnumerable<Transactions>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
      string userId = UserHelper.GetCurrentSignedInUserId();
      endDate = endDate.Date.AddDays(1).AddTicks(-1);

      return await _context.Transactions
        .Where(t => (t.SourceAccountId == userId || t.DestinationAccountId == userId) &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
        .OrderByDescending(t => t.TransactionDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<Transactions>> GetAllTransactionsByInputAsync(string? id, string? accountNumber)
    {
      if (id != null)
      {
        return await _context.Transactions.Where(x => x.SourceAccountId == id).ToListAsync();
      }
      else if (accountNumber != null)
      {
        return await _context.Transactions.Where(x => x.SourceAccountNumber == accountNumber).ToListAsync();
      }
      else
      {
        return [];
      }
    }

    public async Task<Transactions> CreateTransactionAsync(Transactions transaction)
    {
      string userId = UserHelper.GetCurrentSignedInUserId();

      IEnumerable<Account> sourceUserAccounts = await _AccountService.GetAccountsByUserIdAsync(userId);
      Account sourceUserAccount = sourceUserAccounts.FirstOrDefault() ?? throw new UnauthorizedAccessException("No accounts found for user.");
      IEnumerable<Account> AllAccounts = await _AccountService.GetAllAccountsAsync();

      transaction.SourceAccountId = sourceUserAccount.Id.ToString();
      string? destinationAccountId = AllAccounts.FirstOrDefault(x => x.AccountNumber == transaction.DestinationAccountNumber)?.Id.ToString();

      if (transaction.DestinationAccountNumber == sourceUserAccount.AccountNumber)
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
      transaction.ReferenceNumber = $"REF-{Guid.NewGuid().ToString()[..8].ToUpper()}";

      transaction.SourceAccountNumber = sourceUserAccount.AccountNumber;
      transaction.SourceName = sourceUserAccount.User?.FullName ?? "Unknown User";
      transaction.SourceAccountId = sourceUserAccount.Id.ToString();

      transaction.DestinationAccountId = destinationAccountId ?? "Unknown Account Id";
      transaction.RecipientName = AllAccounts.FirstOrDefault(x => x.Id.ToString() == destinationAccountId)?.User?.FullName ?? "Unknown Account Holder";

      transaction.Status = TransactionStatus.Created;
      transaction.Currency = Currency.EUR;
      transaction.PaymentMethod = PaymentMethod.CreditCard;

      transaction.CreatedAt = DateTime.UtcNow;
      transaction.UpdatedAt = DateTime.UtcNow;
      transaction.TransactionDate = DateTime.UtcNow;
      transaction.Metadata = [metadata];

      _context.Transactions.Add(transaction);
      await _context.SaveChangesAsync();

      return transaction;
    }
  }
}