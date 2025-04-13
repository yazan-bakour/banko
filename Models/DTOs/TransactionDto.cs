using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

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
    [Required]
    [SwaggerSchema(Description = "Source account Id")]
    [DefaultValue("")]
    public string? SourceAccountId { get; set; }

    [Required]
    [SwaggerSchema(Description = "Amount of the transaction")]
    [Range(0.01, double.MaxValue)]
    [DefaultValue(0.00)]
    public decimal Amount { get; set; } = 0;

    [Required]
    [SwaggerSchema(Description = "Description of transaction")]
    [DefaultValue("")]
    public string? Description { get; set; } = string.Empty;

    [Required]
    [SwaggerSchema(Description = "Type of transaction")]
    [EnumDataType(typeof(TransactionType))]
    [Column(TypeName = "varchar(20)")]
    [DefaultValue(TransactionType.Deposit)]
    public TransactionType Type { get; set; } = TransactionType.Deposit;

    [Required]
    [SwaggerSchema(Description = "Destination account Id")]
    [DefaultValue("")]
    public string? DestinationAccountId { get; set; } = string.Empty;

    [Required]
    [SwaggerSchema(Description = "Internal/External transaction to account")]
    [DefaultValue(true)]
    public bool IsInternal { get; set; } = false;

    [Required]
    [SwaggerSchema(Description = "Destination account number")]
    [DefaultValue("")]
    public string? AccountNumber { get; set; } = string.Empty;
  }
}