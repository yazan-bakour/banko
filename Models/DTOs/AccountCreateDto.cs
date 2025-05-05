using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Banko.Models.DTOs;
public class AccountCreateDto

{
  [Required]
  public int UserId { get; set; }

  [Required]
  [Range(0, double.MaxValue, ErrorMessage = "Balance must be a positive number")]
  public decimal Balance { get; set; }

  [Required]
  [EnumDataType(typeof(AccountType))]
  [Column(TypeName = "varchar(30)")]
  [DefaultValue(AccountType.Savings)]
  [JsonConverter(typeof(JsonStringEnumConverter<AccountType>))]
  public AccountType Type { get; set; }
}
