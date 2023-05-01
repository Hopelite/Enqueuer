using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Enqueuer.Service.Messages.Validation;

/// <summary>
/// Specifies that the property or field is the queue name, applying an appropriate validations.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public sealed class QueueNameAttribute : ValidationAttribute
{
    private static readonly Regex Regex = new (ValidQueueNameRegex, RegexOptions.Compiled);

    // Check that a queue name doesn't consist only of digits or spaces
    private const string ValidQueueNameRegex = @"(?!^(\d|\s)+$)^.+$";
    private const int MaxQueueNameLength = 64;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string queueName)
        {
            return new ValidationResult("The queueName must have a string type.");
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            return new ValidationResult("The queueName can't be null, empty or a whitespace.");
        }

        if (queueName.Length > MaxQueueNameLength)
        {
            return new ValidationResult("The length of the queueName cannot exceed 64 characters.");
        }

        if (!Regex.IsMatch(queueName))
        {
            return new ValidationResult("The queueName should not consist only of digits and spaces.");
        }

        return ValidationResult.Success;
    }
}
