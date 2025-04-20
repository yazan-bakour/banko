using Banko.Services;
using Microsoft.AspNetCore.Mvc;
using Banko.Models.DTOs;
using Banko.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace Banko.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class TransactionsController(TransactionsService TransactionsService, AccountService AccountService, IMapper Mapper) : ControllerBase
  {
    private readonly TransactionsService _TransactionsService = TransactionsService;
    private readonly AccountService _AccountService = AccountService;
    private readonly IMapper _mapper = Mapper;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionReadDto>>> GetAllTransactionsForUser()
    {
      IEnumerable<Transactions> transactions = await _TransactionsService.GetAllTransactionsByUserIdAsync();

      return Ok(transactions);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<TransactionReadDto>>> GetAllTransactionsForUserByDate(
      [FromQuery] string? period = null,
      [FromQuery] DateTime? fromDate = null,
      [FromQuery] DateTime? toDate = null
    )
    {
      DateTime startDate;
      DateTime endDate = DateTime.UtcNow;

      // Double check this logic
      if (!string.IsNullOrEmpty(period))
      {
        switch (period.ToLowerInvariant())
        {
          case "1m":
          case "last-month":
            startDate = DateTime.UtcNow.AddMonths(-1);
            break;
          case "3m":
          case "last-3-months":
            startDate = DateTime.UtcNow.AddMonths(-3);
            break;
          case "6m":
          case "last-6-months":
            startDate = DateTime.UtcNow.AddMonths(-6);
            break;
          default:
            return BadRequest($"Invalid period parameter: {period}. Valid values are '1m', '3m', '6m'.");
        }
      }
      else if (fromDate.HasValue)
      {
        startDate = fromDate.Value;

        if (toDate.HasValue)
        {
          endDate = toDate.Value;
        }

        if (startDate > endDate)
        {
          return BadRequest("Start date cannot be later than end date.");
        }
      }
      else
      {
        startDate = DateTime.UtcNow.AddMonths(-1);
      }

      IEnumerable<Transactions> transactions = await _TransactionsService.GetTransactionsByDateRangeAsync(startDate, endDate);

      IEnumerable<TransactionReadDto> transactionDtos = _mapper.Map<IEnumerable<TransactionReadDto>>(transactions);

      return Ok(transactionDtos);
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Transactions>>> GetAllTransactionsForAdmin([FromQuery] string? id, [FromQuery] string? accountNumber)
    {
      if (id == null && accountNumber == null)
      {
        return BadRequest("Insert Id or Account Number");
      }
      IEnumerable<Transactions> transactions = await _TransactionsService.GetAllTransactionsByInputAsync(id, accountNumber);

      return Ok(transactions);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateDto transactionCreateDto)
    {
      IEnumerable<Account> existingAccounts = await _AccountService.GetAllAccountsAsync();

      bool existingAccountNumber = existingAccounts.Any(a => a.AccountNumber == transactionCreateDto.AccountNumber);

      if (!existingAccountNumber)
      {
        // For now only internal accounts are supported.
        return BadRequest("Account number does not match the existing accounts. Only internal accounts are supported.");
      }

      Transactions transaction = _mapper.Map<Transactions>(transactionCreateDto);

      // enhance this logic based on status
      var createdTransaction = await _TransactionsService.CreateTransactionAsync(transaction);

      return Ok(new { Message = "Transaction created successfully.", TransactionId = createdTransaction.Id });
    }
  }
}