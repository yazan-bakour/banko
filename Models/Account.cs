using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Banko.Helpers;

namespace Banko.Models
{
  public class Account : BaseEntity
  {
    [Required]
    public int UserId { get; set; }

    [Required]
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Required]
    [MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    public decimal Balance { get; set; } = 0.0m;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [EnumDataType(typeof(AccountType))]
    [Column(TypeName = "varchar(30)")]
    [DefaultValue(AccountType.Savings)]
    [JsonConverter(typeof(JsonStringEnumConverter<AccountType>))]
    public AccountType Type { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal InterestRate { get; set; } = 0.0m;

    [Required]
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [EnumDataType(typeof(AccountStatus))]
    [Column(TypeName = "varchar(10)")]
    [DefaultValue(AccountStatus.Active)]
    [JsonConverter(typeof(JsonStringEnumConverter<AccountStatus>))]
    public AccountStatus Status { get; set; }

    [Required]
    [EnumDataType(typeof(Currency))]

    [DefaultValue(Currency.EUR)]
    [JsonConverter(typeof(JsonStringEnumConverter<Currency>))]
    public Currency Currency { get; set; }

    public DateTime? LastTransactionDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumBalance { get; set; } = 0.0m;

    [Range(0, double.MaxValue)]
    public decimal OverdraftLimit { get; set; } = 0.0m;
  }
  public enum AccountType
  {
    Checking,
    Savings,
    Investment,
    Business,
    MoneyMarket,
    CertificateOfDeposit,
    RetirementAccount,
    StudentAccount,
    JointAccount,
    Trust
  }
  public enum AccountStatus
  {
    Active,
    Inactive,
    Frozen,
    Closed,
    PendingApproval
  }
}
