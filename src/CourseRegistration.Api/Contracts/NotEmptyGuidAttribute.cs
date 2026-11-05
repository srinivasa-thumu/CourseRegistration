using System.ComponentModel.DataAnnotations;

namespace CourseRegistration.Api.Contracts.Validation;

public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is Guid id && id != Guid.Empty;
    }
}