using Banko.Services;
using Microsoft.AspNetCore.Mvc;
using Banko.Models.DTOs;
using Banko.Models;

// think of a better way for internal and external transactions

namespace Banko.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TransactionsController(TransactionsService TransactionsService, AccountService AccountService) : ControllerBase
  {
    private readonly TransactionsService _TransactionsService = TransactionsService;
    private readonly AccountService _AccountService = AccountService;

    [HttpPost()]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto transactionDto)
    {
      var existingAccounts = await _AccountService.GetAllAccountsAsync();
      bool existingAccountNumber = existingAccounts.Any(a => a.AccountNumber == transactionDto.AccountNumber);

      if (!transactionDto.IsInternal && string.IsNullOrEmpty(transactionDto.AccountNumber))
      {
        ModelState.AddModelError("AccountNumber", "Account Number is required");
        return BadRequest(ModelState);
      }
      // else if (transactionDto.IsInternal && string.IsNullOrEmpty(transactionDto.DestinationAccountId))
      // {
      //   ModelState.AddModelError("AccountId", "Account ID is required for internal transactions");
      //   return BadRequest(ModelState);
      // }

      Transactions transaction = new()
      {
        TransactionNumber = Guid.NewGuid().ToString(),
        Amount = transactionDto.Amount,
        Description = transactionDto.Description,
        Type = transactionDto.Type,
        IsInternal = transactionDto.IsInternal,
        AccountNumber = transactionDto.AccountNumber,
        DestinationAccountId = transactionDto.DestinationAccountId,
      };


      if (!existingAccountNumber)
      {
        return BadRequest("Account number does not match the existing accounts.");
      }

      var createdTransaction = await _TransactionsService.CreateTransactionAsync(transaction);

      return Ok(new { Message = "Transaction created successfully.", TransactionId = createdTransaction.Id });
    }
  }
}