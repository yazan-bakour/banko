using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Banko.Models.DTOs
{
  public class TransactionCreateDto
  {
    [Required]
    [DefaultValue("")]
    public required string DestinationAccountNumber { get; set; }

    [Required]
    [DefaultValue("")]
    public required string SourceAccountNumber { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    [DefaultValue(0.00)]
    public decimal Amount { get; set; } = 0;

    [Required]
    [EnumDataType(typeof(TransactionType))]
    [Column(TypeName = "varchar(30)")]
    [DefaultValue(TransactionType.Deposit)]
    [JsonConverter(typeof(JsonStringEnumConverter<TransactionType>))]
    public TransactionType Type { get; set; } = TransactionType.Deposit;

    [Required]
    [DefaultValue("")]
    public string? Description { get; set; } = string.Empty;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }
  }
}