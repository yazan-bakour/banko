using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// rename the class to create.
// add status logics.
// deduct the amount from source.
// add the amount to destination balance.
// convert enums from int to string in the database.
// add externalAccount and internalAccount instead of isInternal.
// add bank name.

namespace Banko.Models.DTOs
{
  public class TransactionDto
  {
    public string? SourceAccountId { get; set; }

    [DefaultValue(0)]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; } = 0;

    [DefaultValue("")]
    public string? Description { get; set; } = string.Empty;

    public TransactionType Type { get; set; } = TransactionType.Deposit;

    public string? DestinationAccountId { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false;
    public string? AccountNumber { get; set; } = string.Empty;
  }
}