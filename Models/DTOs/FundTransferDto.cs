using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Banko.Models.DTOs
{
  public class FundTransferDto
  {
    [Required]
    public int SourceId { get; set; }

    [Required]
    public int DestinationId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required]
    [DefaultValue("")]
    public string? Message { get; set; }
  }
}