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
  public class TransactionsService(UserHelper _userHelper, AppDbContext _context, IHttpContextAccessor _httpContextAccessor)
  {
    public async Task<IEnumerable<Transactions>> GetAllTransactionsByUserIdAsync()
    {
      string userId = _userHelper.GetCurrentSignedInUserId();
      if (!int.TryParse(userId, out int parsedUserId))
      {
        throw new InvalidOperationException("Invalid user ID format.");
      }
      List<string> userAccountIds = await _context.Accounts
                                        .Where(a => a.UserId == parsedUserId)
                                        .Select(a => a.Id.ToString())
                                        .ToListAsync();

      return await _context.Transactions
        .Where(t => userAccountIds.Contains(t.SourceAccountId) || userAccountIds.Contains(t.DestinationAccountId))
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
    }

    public async Task<IEnumerable<Transactions>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
      string userId = _userHelper.GetCurrentSignedInUserId();
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

    public async Task<ServiceResult<Transactions>> CreateTransactionAsync(Transactions transaction)
    {
      string userId = _userHelper.GetCurrentSignedInUserId();
      if (!int.TryParse(userId, out int parsedUserId))
      {
        return ServiceResult<Transactions>.Failure("Invalid user ID format.");
      }

      Account? sourceUserAccount = await _context.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.UserId == parsedUserId && a.AccountNumber == transaction.SourceAccountNumber);
      Account? destinationAccount = await _context.Accounts.Include(a => a.User).FirstOrDefaultAsync(x => x.AccountNumber == transaction.DestinationAccountNumber);

      if (destinationAccount == null)
      {
        // We only allow transaction for internal accounts for now.
        return ServiceResult<Transactions>.Failure("The destination account does not exist.");
      }
      if (sourceUserAccount?.Balance < transaction.Amount)
      {
        return ServiceResult<Transactions>.Failure("Insufficient funds.");
      }
      if (transaction.Amount <= 0)
      {
        return ServiceResult<Transactions>.Failure("Transaction amount must be positive.");
      }
      if (transaction.DestinationAccountNumber == sourceUserAccount?.AccountNumber)
      {
        return ServiceResult<Transactions>.Failure("You cannot transfer to your own account.");
      }
      // if (!sourceUserAccount.IsActive || sourceUserAccount.Status != AccountStatus.Active)
      // {
      //   return ServiceResult<Transactions>.Failure("Source account is not active.");
      // }
      // if (!destinationAccount.IsActive || destinationAccount.Status != AccountStatus.Active)
      // {
      //   return ServiceResult<Transactions>.Failure("Destination account is not active.");
      // }

      string ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
      Metadata metadata = new()
      {
        IpAddress = ipAddress,
        Device = _httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString() ?? "Unknown",
        Location = await _userHelper.GetLocationAsync(ipAddress)
      };

      transaction.Id = Guid.NewGuid();
      transaction.ReferenceNumber = $"REF-{Guid.NewGuid().ToString()[..8].ToUpper()}";
      transaction.TransactionNumber = $"T-{DateTime.UtcNow.Ticks}-{transaction.ReferenceNumber}";

      transaction.SourceAccountNumber = sourceUserAccount!.AccountNumber;
      transaction.SourceName = sourceUserAccount.User?.FullName ?? "Unknown User";
      transaction.SourceAccountId = sourceUserAccount.Id.ToString();

      transaction.DestinationAccountNumber = destinationAccount!.AccountNumber;
      transaction.DestinationAccountId = destinationAccount!.Id.ToString();
      transaction.RecipientName = destinationAccount?.User?.FullName ?? "Unknown Account Holder";

      // once we have other services such as check in and out, we can adjust the status based on the bank approvals.
      transaction.Status = TransactionStatus.Completed;
      transaction.Currency = sourceUserAccount.Currency;

      transaction.CreatedAt = DateTime.UtcNow;
      transaction.UpdatedAt = DateTime.UtcNow;
      transaction.TransactionDate = DateTime.UtcNow;
      transaction.Metadata = [metadata];

      sourceUserAccount.Balance -= transaction.Amount;
      destinationAccount!.Balance += transaction.Amount;
      sourceUserAccount.UpdatedAt = DateTime.UtcNow;
      destinationAccount.UpdatedAt = DateTime.UtcNow;
      sourceUserAccount.LastTransactionDate = DateTime.UtcNow;
      destinationAccount.LastTransactionDate = DateTime.UtcNow;

      _context.Transactions.Add(transaction);
      await _context.SaveChangesAsync();

      return ServiceResult<Transactions>.Success(transaction);
    }
  }
}