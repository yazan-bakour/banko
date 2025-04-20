using System.ComponentModel.DataAnnotations;

namespace Banko.Models.DTOs
{
  public class TransactionReadDto
  {
    public string? TransactionNumber { get; set; }
    public string? SourceName { get; set; }
    public string? SourceAccountNumber { get; set; }
    public string? RecipientName { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string? TransactionStatusName { get; set; }

    [EnumDataType(typeof(TransactionStatus))]
    public TransactionStatus Status { get; set; }

    [EnumDataType(typeof(TransactionType))]
    public TransactionType Type { get; set; }

    [EnumDataType(typeof(TransactionType))]
    public string? TransactionTypeName { get; set; }
    public decimal Amount { get; set; }

    [EnumDataType(typeof(Currency))]
    public string? CurrencyName { get; set; }

    [EnumDataType(typeof(Currency))]
    public Currency Currency { get; set; }

    [EnumDataType(typeof(PaymentMethod))]
    public PaymentMethod PaymentMethod { get; set; }

    [EnumDataType(typeof(PaymentMethod))]
    public string? PaymentMethodName { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
  }
}