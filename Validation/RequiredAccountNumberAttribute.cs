using System.ComponentModel.DataAnnotations;
using Banko.Models;

// test the attribute

namespace Banko.Validation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
  public class RequiredAccountNumberAttribute : ValidationAttribute
  {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
      Transactions transaction = (Transactions)validationContext.ObjectInstance;

      if (!transaction.IsInternal && string.IsNullOrEmpty(transaction.AccountNumber))
      {
        return new ValidationResult("AccountNumber is required for external transactions.");
      }

      return ValidationResult.Success;
    }
  }
}