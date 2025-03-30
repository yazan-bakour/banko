using Banko.Models;
using Banko.Models.DTOs;
using Banko.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// TODO: Add test.
// TODO: Remove messages from responses.

namespace Banko.Controllers
{
  [ApiController]
  [Route("api/accounts")]
  public class AccountController(AccountService accountService, UserService userService) : ControllerBase
  {
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
    {
      var accounts = await accountService.GetAllAccountsAsync();
      return Ok(accounts);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Support")]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
      var account = await accountService.GetAccountByIdAsync(id);
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
      var user = await userService.GetUserByIdAsync(request.UserId);
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

      var createdAccount = await accountService.CreateAccountAsync(account);

      if (createdAccount == null)
      {
        return BadRequest(new { message = "Failed to create account" });
      }

      return CreatedAtAction(
        nameof(GetAccount),
        new { id = createdAccount.Id },
        new
        {
          accountId = createdAccount.Id,
          email = createdAccount.User?.Email,
          userName = createdAccount.User?.FullName,
          accountNumber = createdAccount.AccountNumber,
          balance = createdAccount.Balance
        }
      );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Account>> UpdateAccount(int id, [FromBody] decimal balance)
    {
      var account = await accountService.UpdateAccountAsync(id, balance);
      if (account == null)
      {
        return NotFound(new { message = "Account not found" });
      }

      return Ok(new { account });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Account>> DeleteAccount(int id)
    {
      var deleted = await accountService.DeleteAccountAsync(id);
      if (!deleted)
      {
        return NotFound();
      }
      return Ok(new { message = "Account deleted successfully" });
    }
  }
}