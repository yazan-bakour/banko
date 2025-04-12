using Banko.Data;
using Banko.Models;
using Microsoft.EntityFrameworkCore;

namespace Banko.Services
{
  public class FundsService(AppDbContext context)
  {
    private readonly AppDbContext _context = context;

    public async Task<Funds?> GetFundsByIdAsync(int id)
    {
      return await _context.Funds.FindAsync(id);
    }

    public async Task<List<Funds>> GetAllFundsAsync()
    {
      return await _context.Funds.Where(f => f.IsActive).ToListAsync();
    }

    public async Task<Funds> CreateFundsAsync(Funds funds)
    {
      _context.Funds.Add(funds);
      await _context.SaveChangesAsync();
      return funds;
    }

    public async Task<bool> TransferFundsAsync(int sourceId, int destinationId, decimal amount, string? message = null)
    {
      Funds? sourceFunds = await _context.Funds.FindAsync(sourceId);
      Funds? destinationFunds = await _context.Funds.FindAsync(destinationId);

      if (sourceFunds == null || destinationFunds == null || !sourceFunds.IsActive || !destinationFunds.IsActive)
      {
        return false;
      }

      if (sourceFunds.Balance < amount)
      {
        return false;
      }

      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        sourceFunds.Balance -= amount;
        sourceFunds.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(message))
        {
          sourceFunds.Message = $"Transfer out: {message}";
        }

        destinationFunds.Balance += amount;
        destinationFunds.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(message))
        {
          destinationFunds.Message = $"Transfer in: {message}";
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
      }
      catch
      {
        await transaction.RollbackAsync();
        return false;
      }
    }

    public async Task<bool> UpdateFundsAsync(int id, decimal newBalance)
    {
      Funds? funds = await _context.Funds.FindAsync(id);
      if (funds == null || !funds.IsActive)
      {
        return false;
      }

      funds.Balance = newBalance;
      funds.UpdatedAt = DateTime.UtcNow;
      await _context.SaveChangesAsync();
      return true;
    }

    public async Task<bool> DeactivateFundsAsync(int id)
    {
      Funds? funds = await _context.Funds.FindAsync(id);
      if (funds == null)
      {
        return false;
      }

      funds.IsActive = false;
      funds.UpdatedAt = DateTime.UtcNow;
      await _context.SaveChangesAsync();
      return true;
    }
  }
}