using System.ComponentModel.DataAnnotations;

namespace Banko.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class PastDateAttribute : ValidationAttribute
{
  public override bool IsValid(object? value)
  {
    // Null values are valid since the property is optional
    if (value == null)
      return true;

    return value is DateTime dateTime && dateTime < DateTime.UtcNow;
  }
}