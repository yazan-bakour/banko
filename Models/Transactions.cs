using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;

// move metadata to it's own class
// move validations from controller to attributes 

namespace Banko.Models
{
  public class Transactions
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [StringLength(20)]
    public string? AccountNumber { get; set; }

    [Required]
    public string? TransactionNumber { get; set; }

    [Range(0.01, double.MaxValue)]
    [DefaultValue(0.00)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    [DefaultValue(TransactionType.Deposit)]
    [EnumDataType(typeof(TransactionType))]
    [Column(TypeName = "varchar(20)")]
    public TransactionType Type { get; set; }

    [EnumDataType(typeof(TransactionStatus))]
    [Column(TypeName = "varchar(20)")]
    public TransactionStatus Status { get; set; }

    public string? Description { get; set; }

    [DefaultValue(Currency.EUR)]
    [EnumDataType(typeof(Currency))]
    [Column(TypeName = "varchar(3)")]
    public Currency Currency { get; set; } = Currency.EUR;

    [StringLength(10)]
    public string? SourceAccountId { get; set; }

    [StringLength(10)]
    public string? DestinationAccountId { get; set; }

    [EnumDataType(typeof(PaymentMethod))]
    [Column(TypeName = "varchar(20)")]
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
    [Description("Deposit")]
    Deposit,

    [Description("Withdrawal")]
    Withdrawal,

    [Description("Transfer")]
    Transfer,

    [Description("Payment")]
    Payment,

    [Description("Refund")]
    Refund,

    [Description("Fee")]
    Fee,

    [Description("Interest")]
    Interest
  }

  public enum PaymentMethod
  {
    [Description("Credit Card Payment")]
    CreditCard,
    [Description("Debit Card Payment")]
    DebitCard,
    [Description("Bank Transfer")]
    BankTransfer,
    [Description("SEBA Transfer")]
    SEBA,
    [Description("Wire Transfer")]
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
    Created,
    Pending,
    Completed,
    Failed,
    Cancelled,
    Refunded,
    Disputed
  }
}