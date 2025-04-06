using Banko.Data;
using Banko.Models;
using Microsoft.EntityFrameworkCore;

// TODO: move GenerateAccountNumber to helper.
// TODO: Use explicit type declaration instead of var

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

    public async Task<Account?> GetAccountByIdAsync(int id)
    {
      return await _context.Accounts
        .Include(a => a.User)
        .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {
      _context.Accounts.Add(account);
      await _context.SaveChangesAsync();
      return account;
    }

    public async Task<Account?> UpdateAccountAsync(int id, decimal balance)
    {
      var account = await GetAccountByIdAsync(id);
      if (account == null) return null;

      account.Balance = balance;
      _context.Accounts.Update(account);
      await _context.SaveChangesAsync();
      return account;
    }

    public async Task<bool> DeleteAccountAsync(int id)
    {
      var account = await GetAccountByIdAsync(id);
      if (account == null) return false;

      _context.Accounts.Remove(account);
      await _context.SaveChangesAsync();
      return true;
    }

    public static string GenerateAccountNumber()
    {
      Random random = new();
      return "NKO" + random.Next(10000000, 99999999).ToString();
    }
  }
}
