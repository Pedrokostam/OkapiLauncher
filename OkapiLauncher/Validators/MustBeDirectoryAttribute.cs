using System.ComponentModel.DataAnnotations;
using System.IO;

namespace OkapiLauncher.Validators;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class MustBeDirectoryAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string s)
        {
            return new ValidationResult("Path must be a string");
        }
        if (File.Exists(s))
        {
            return new ValidationResult("Path must be a directory");
        }
        return ValidationResult.Success;
    }
}
