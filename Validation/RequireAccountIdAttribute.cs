using System.ComponentModel.DataAnnotations;
using Banko.Data;
// using Banko.Models;

// refactor this attribute

namespace Banko.Validation
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]

  public class RequireAccountIdAttribute : ValidationAttribute
  {
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
      if (validationContext.GetService(typeof(AppDbContext)) is not AppDbContext dbContext)
      {
        return new ValidationResult("Service not available.");
      }
      // Transactions transaction = (Transactions)validationContext.ObjectInstance;

      // if (transaction.IsInternal)
      // {
      //   return new ValidationResult("AccountNumber is required for external transactions.");
      // }

      if (!int.TryParse(value?.ToString(), out int accountId))
      {
        return new ValidationResult("The Account ID must be an integer.");
      }
      bool accountExists = dbContext.Accounts.Any(a => a.Id == accountId);
      if (!accountExists)
      {
        return new ValidationResult("The specified Account ID does not exist.");
      }

      return ValidationResult.Success!;
    }
  }
}