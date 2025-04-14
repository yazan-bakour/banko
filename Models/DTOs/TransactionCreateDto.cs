using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// add status logics.
// deduct the amount from source.
// add the amount to destination balance.
// convert enums from int to string in the database.
// add externalAccount and internalAccount instead of isInternal.
// add bank name.

namespace Banko.Models.DTOs
{
  public class TransactionCreateDto
  {
    [Required]
    [DefaultValue("")]
    public string? AccountNumber { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    [DefaultValue(0.00)]
    public decimal Amount { get; set; } = 0;

    [Required]
    [DefaultValue("")]
    public string? Description { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(TransactionType))]
    [Column(TypeName = "varchar(20)")]
    [DefaultValue(TransactionType.Deposit)]
    public TransactionType Type { get; set; } = TransactionType.Deposit;
  }
}