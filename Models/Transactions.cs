using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Banko.Validation;

// move metadata to it's own class

namespace Banko.Models
{
  public class Transactions
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public bool IsInternal { get; set; }

    [RequiredAccountNumber]
    [StringLength(20, ErrorMessage = "Account number cannot exceed 20 characters.")]
    public string? AccountNumber { get; set; }

    [Required]
    public string? TransactionNumber { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    public TransactionType Type { get; set; }

    public TransactionStatus Status { get; set; }

    public string? Description { get; set; }

    public Currency Currency { get; set; } = Currency.EUR;

    public string? UserId { get; set; }

    public string? SourceAccountId { get; set; }

    [RequireAccountId]
    public string? DestinationAccountId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? ReferenceNumber { get; set; }

    public ICollection<Metadata> Metadata { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
  }

  public class Metadata
  {
    [Key]
    public int Id { get; set; }

    public string IpAddress { get; set; } = "Unknown";
    public string Device { get; set; } = "Unknown";
    public string Location { get; set; } = "Unknown";

    public Guid TransactionId { get; set; }

    [ForeignKey("TransactionId")]
    public Transactions? Transaction { get; set; }
  }

  public enum TransactionType
  {
    Deposit,
    Withdrawal,
    Transfer,
    Payment,
    Refund,
    Fee,
    Interest
  }

  public enum PaymentMethod
  {
    CreditCard,
    DebitCard,
    BankTransfer,
    SEBA,
    Wire,
  }

  public enum Currency
  {
    USD,
    EUR,
    GBP,
    JPY,
    AUD,
    CAD,
    CHF,
    CNY,
    SEK,
    NZD
  }

  public enum TransactionStatus
  {
    Pending,
    Completed,
    Failed,
    Cancelled,
    Refunded,
    Disputed
  }
}