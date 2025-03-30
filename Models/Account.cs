using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Banko.Models
{
  public class Account
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

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
  }
}
