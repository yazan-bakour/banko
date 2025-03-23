using Banko.Data;
using Banko.Models;
using Banko.Models.DTOs;
using Banko.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Banko.Controllers
{
  [ApiController]
  [Route("api/accounts")]
  public class AccountController(AppDbContext context) : ControllerBase
  {
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
    {
      var accounts = await _context.Accounts.ToListAsync();
      return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
      var account = await _context.Accounts.FindAsync(id);
      if (account == null)
      {
        return NotFound();
      }
      return Ok(account);
    }

    [HttpPost]
    [Route("create")]
    public async Task<ActionResult<Account>> CreateAccount([FromBody] AccountCreateDto request)
    {
      var user = await _context.Users.FindAsync(request.UserId);
      if (user == null)
      {
        return NotFound(new { message = "User not found" });
      }

      var accountNumber = AccountService.GenerateAccountNumber();

      var account = new Account
      {
        UserId = request.UserId,
        User = user,
        AccountNumber = accountNumber,
        Balance = request.Balance,
        CreatedAt = DateTime.UtcNow
      };

      _context.Accounts.Add(account);
      await _context.SaveChangesAsync();

      return CreatedAtAction(
        nameof(GetAccount),
        new { id = account.Id },
        new { accountId = account.Id, email = account.User.Email, userName = account.User.FullName, accountNumber = account.AccountNumber, balance = account.Balance }
      );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Account>> UpdateAccount(int id, [FromBody] decimal balance)
    {
      var account = await _context.Accounts.FindAsync(id);
      if (account == null)
      {
        return NotFound(new { message = "Account not found" });
      }
      // For now I update the balance of the account
      account.Balance = balance;
      _context.Accounts.Update(account);
      await _context.SaveChangesAsync();

      return Ok(new { account });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Account>> DeleteAccount(int id)
    {
      var accountResult = await GetAccount(id);
      if (accountResult.Value == null)
      {
        return NotFound();
      }
      _context.Accounts.Remove(accountResult.Value);
      await _context.SaveChangesAsync();
      return Ok(new { message = "Account deleted successfully" });
    }
  }
}