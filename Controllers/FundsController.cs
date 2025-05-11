using Banko.Helpers;
using Banko.Models;
using Banko.Models.DTOs;
using Banko.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// TODO: Add validation

namespace Banko.Controllers
{
  [Authorize]
  [ApiController]
  [Route("api/[controller]")]
  public class FundsController(FundsService fundsService, UserHelper userHelper) : ControllerBase
  {
    private readonly FundsService _fundsService = fundsService;

    [HttpGet]
    public async Task<ActionResult<List<Funds>>> GetAllFunds()
    {
      List<Funds> funds = await _fundsService.GetAllFundsAsync();
      return Ok(funds);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Funds>> GetFundsById(int id)
    {
      Funds? funds = await _fundsService.GetFundsByIdAsync(id);
      if (funds == null)
      {
        return NotFound();
      }
      return Ok(funds);
    }

    [HttpPost]
    public async Task<ActionResult<Funds>> CreateFunds(FundsCreateDto funds)
    {

      IEnumerable<Funds> existingFunds = await _fundsService.GetAllFundsAsync();
      bool isDuplicateId = existingFunds.Any(existingFund => existingFund.Name == funds.Name);
      string currentUserId = userHelper.GetCurrentSignedInUserId();

      Funds newFunds = new()
      {
        UserId = int.Parse(currentUserId), // the creator of the fund.
        AccountId = funds.AccountId, // select account were you want to create fund.
        Name = funds.Name,
        Balance = funds.Balance,
        Description = funds.Description
      };

      if (isDuplicateId)
      {
        return BadRequest("Fund already exists. Please use a different name.");
      }

      Funds createdFund = await _fundsService.CreateFundsAsync(newFunds);
      return CreatedAtAction(
        actionName: nameof(GetFundsById),
        routeValues: new { id = createdFund.Id },
        value: createdFund
      );
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferFunds([FromBody] FundTransferDto transferRequest)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      bool success = await _fundsService.TransferFundsAsync(
        transferRequest.SourceId,
        transferRequest.DestinationId,
        transferRequest.Amount,
        transferRequest.Message
      );

      if (!success)
      {
        return BadRequest("Transfer failed. Please check the account IDs and balances.");
      }

      return Ok("Transfer completed successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFunds(int id, decimal newBalance)
    {
      if (newBalance < 0)
      {
        return BadRequest("Balance cannot be negative");
      }

      bool success = await _fundsService.UpdateFundsAsync(id, newBalance);
      if (!success)
      {
        return NotFound();
      }

      return Ok("Funds updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateFunds(int id)
    {
      bool success = await _fundsService.DeactivateFundsAsync(id);
      if (!success)
      {
        return NotFound();
      }

      return Ok("Funds deactivated successfully");
    }
  }
}