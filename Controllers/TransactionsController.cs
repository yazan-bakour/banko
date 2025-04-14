using Banko.Services;
using Microsoft.AspNetCore.Mvc;
using Banko.Models.DTOs;
using Banko.Models;
using Microsoft.AspNetCore.Authorization;

namespace Banko.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class TransactionsController(TransactionsService TransactionsService, AccountService AccountService) : ControllerBase
  {
    private readonly TransactionsService _TransactionsService = TransactionsService;
    private readonly AccountService _AccountService = AccountService;

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateDto transactionCreateDto)
    {
      IEnumerable<Account> existingAccounts = await _AccountService.GetAllAccountsAsync();

      bool existingAccountNumber = existingAccounts.Any(a => a.AccountNumber == transactionCreateDto.AccountNumber);

      Transactions transaction = new()
      {
        TransactionNumber = Guid.NewGuid().ToString(),
        Amount = transactionCreateDto.Amount,
        Description = transactionCreateDto.Description,
        Type = transactionCreateDto.Type,
        AccountNumber = transactionCreateDto.AccountNumber,
      };

      if (!existingAccountNumber)
      {
        // For now only internal accounts are supported.
        return BadRequest("Account number does not match the existing accounts. Only internal accounts are supported.");
      }

      // enhance this logic based on status
      var createdTransaction = await _TransactionsService.CreateTransactionAsync(transaction);

      return Ok(new { Message = "Transaction created successfully.", TransactionId = createdTransaction.Id });
    }
  }
}