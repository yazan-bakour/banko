using Banko.Services;
using Microsoft.AspNetCore.Mvc;
using Banko.Models.DTOs;
using Banko.Models;
using Microsoft.AspNetCore.Authorization;

// think of a better way for internal and external transactions

namespace Banko.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(Roles = "Customer,Support,Admin")]
  public class TransactionsController(TransactionsService TransactionsService, AccountService AccountService) : ControllerBase
  {
    private readonly TransactionsService _TransactionsService = TransactionsService;
    private readonly AccountService _AccountService = AccountService;

    [HttpPost()]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto transactionDto)
    {
      IEnumerable<Account> existingAccounts = await _AccountService.GetAllAccountsAsync();

      bool existingAccountNumber = existingAccounts.Any(a => a.AccountNumber == transactionDto.AccountNumber);
      bool existingAccountDestinationId = existingAccounts.Any(a => a.Id.ToString() == transactionDto.DestinationAccountId);
      bool existingAccountSourceId = existingAccounts.Any(a => a.Id.ToString() == transactionDto.SourceAccountId);

      var existingAccountBalance = existingAccounts.Where(a => a.AccountNumber == transactionDto.AccountNumber).Select(a => a.Balance).FirstOrDefault();

      Transactions transaction = new()
      {
        TransactionNumber = Guid.NewGuid().ToString(),
        Amount = transactionDto.Amount,
        Description = transactionDto.Description,
        Type = transactionDto.Type,
        IsInternal = transactionDto.IsInternal,
        AccountNumber = transactionDto.AccountNumber,
        DestinationAccountId = transactionDto.DestinationAccountId,
        SourceAccountId = transactionDto.SourceAccountId,
      };

      // if (transaction.Amount == 0)
      // {
      //   return BadRequest("The transaction Amount must be higher than 0.");
      // }

      if (existingAccountSourceId)
      {
        if (existingAccountBalance < transaction.Amount)
        {
          ModelState.AddModelError("Amount", "Insufficient funds.");
          return BadRequest(ModelState);
        }
        else
        {
          existingAccountBalance -= transaction.Amount;
          await _AccountService.UpdateAccountAsync(int.Parse(transaction.SourceAccountId ?? "0"), existingAccountBalance);
        }
      }
      else
      {
        ModelState.AddModelError("SourceAccountId", "Source Account ID does not exist.");
        return BadRequest(ModelState);
      }

      if (!existingAccountDestinationId)
      {
        ModelState.AddModelError("DestinationAccountId", "Destination Account ID does not exist.");
        return BadRequest(ModelState);
      }

      if (!existingAccountNumber)
      {
        return BadRequest("Account number does not match the existing accounts.");
      }

      transaction.Status = TransactionStatus.Created;
      // enhance this logic based on status
      var createdTransaction = await _TransactionsService.CreateTransactionAsync(transaction);

      return Ok(new { Message = "Transaction created successfully.", TransactionId = createdTransaction.Id });
    }
  }
}