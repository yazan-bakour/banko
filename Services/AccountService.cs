using Banko.Data;
using Banko.Models;
using Microsoft.EntityFrameworkCore;

namespace Banko.Services
{
  public class AccountService(AppDbContext context)
  {
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<Account>> GetAllAccountsAsync()
    {
      return await _context.Accounts
        .Include(a => a.User)
        .ToListAsync();
    }

    public async Task<Account?> GetAccountByAccountIdAsync(int id)
    // Change to GetAccounts and return list of accounts.
    {
      return await _context.Accounts
        .Include(a => a.User)
        .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Account>> GetAccountsByUserIdAsync(string userId)
    {
      return await _context.Accounts
          .Where(a => a.UserId.ToString() == userId)
          .ToListAsync();
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {
      _context.Accounts.Add(account);
      await _context.SaveChangesAsync();
      return account;
    }

    public async Task<Account?> UpdateAccountAsync(int id, decimal balance)
    {
      Account? account = await GetAccountByAccountIdAsync(id);
      if (account == null) return null;

      account.Balance = balance;
      _context.Accounts.Update(account);
      await _context.SaveChangesAsync();
      return account;
    }

    public async Task<Account?> UpdateAccountStatusAsync(int id, AccountStatus newStatus)
    {
      Account? account = await _context.Accounts.FindAsync(id);
      if (account == null)
      {
        return null;
      }

      account.Status = newStatus;
      account.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();
      return account;
    }

    public async Task<bool> DeleteAccountAsync(int id)
    {
      Account? account = await GetAccountByAccountIdAsync(id);
      if (account == null) return false;

      _context.Accounts.Remove(account);
      await _context.SaveChangesAsync();
      return true;
    }
  }
}
