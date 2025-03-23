using Banko.Data;
using Banko.Models;
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
    public async Task<ActionResult<Account>> CreateAccount(Account account)
    {
      _context.Accounts.Add(account);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, new { message = "Account created successfully", account });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Account>> UpdateAccount(int id)
    {
      var accountResult = await GetAccount(id);
      if (accountResult.Value == null)
      {
        return NotFound();
      }
      _context.Accounts.Update(accountResult.Value);
      await _context.SaveChangesAsync();
      return Ok(new { message = "Account updated successfully", account = accountResult.Value });
    }

    [HttpDelete("{id}")]
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