using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Banko.Models.DTOs
{
  public class FundsCreateDto
  {
    [Required]
    [DefaultValue("")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DefaultValue(0)]
    public decimal Balance { get; set; } = 0;

    [Required]
    [DefaultValue("")]
    public required string Description { get; set; } = string.Empty;
  }
}