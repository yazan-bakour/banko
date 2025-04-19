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
    public TransactionStatus Status { get; set; }
    public TransactionType Type { get; set; }
    public string? TransactionTypeName { get; set; }
    public decimal Amount { get; set; }
    public string? CurrencyName { get; set; }
    public Currency Currency { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentMethodName { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
  }
}