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
  public class TransactionsController(TransactionsService TransactionsService, IMapper Mapper) : ControllerBase
  {
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionReadDto>>> GetAllTransactionsForUser()
    {
      IEnumerable<Transactions> transactions = await TransactionsService.GetAllTransactionsByUserIdAsync();
      IEnumerable<TransactionReadDto> transactionDtos = Mapper.Map<IEnumerable<TransactionReadDto>>(transactions);
      return Ok(transactionDtos);
    }
    // Keep for future sorting, filtering
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

      IEnumerable<Transactions> transactions = await TransactionsService.GetTransactionsByDateRangeAsync(startDate, endDate);

      IEnumerable<TransactionReadDto> transactionDtos = Mapper.Map<IEnumerable<TransactionReadDto>>(transactions);

      return Ok(transactionDtos);
    }

    [HttpGet("history/pdf")]
    public async Task<IActionResult> DownloadStatementPdf(
    [FromQuery] string? period = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null)
    {
      DateTime startDate;
      DateTime endDate = DateTime.UtcNow;

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

      // Fetch transactions
      var transactions = await TransactionsService.GetTransactionsByDateRangeAsync(startDate, endDate);

      // Generate PDF using QuestPDF
      var pdfBytes = GeneratePdfService.GenerateStatementPdf(transactions, startDate, endDate);

      var fileName = $"statement_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
      return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Transactions>>> GetAllTransactionsForAdmin([FromQuery] string? id, [FromQuery] string? accountNumber)
    {
      if (id == null && accountNumber == null)
      {
        return BadRequest("Insert Id or Account Number");
      }
      IEnumerable<Transactions> transactions = await TransactionsService.GetAllTransactionsByInputAsync(id, accountNumber);

      return Ok(transactions);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionReadDto>> CreateTransaction([FromBody] TransactionCreateDto transactionCreateDto)
    {
      Transactions transaction = Mapper.Map<Transactions>(transactionCreateDto);
      // enhance this logic based on status once admin platform built.
      ServiceResult<Transactions> serviceResult = await TransactionsService.CreateTransactionAsync(transaction);

      if (!serviceResult.IsSuccess)
      {
        if (serviceResult.ErrorMessage == "Invalid user ID format.")
        {
          return Unauthorized(new { message = serviceResult.ErrorMessage });
        }

        return BadRequest(new { message = serviceResult.ErrorMessage, errors = serviceResult.Errors });
      }

      TransactionReadDto transactionReadDto = Mapper.Map<TransactionReadDto>(serviceResult.Data);

      return Ok(transactionReadDto);
    }
  }
}