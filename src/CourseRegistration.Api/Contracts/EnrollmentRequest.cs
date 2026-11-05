using System.ComponentModel.DataAnnotations;
using CourseRegistration.Api.Contracts.Validation;

namespace CourseRegistration.Api.Contracts;

public sealed class EnrollmentRequest
{
    [NotEmptyGuid(ErrorMessage = "Course offering ID is required.")]
    public Guid CourseOfferingId { get; init; }

    [NotEmptyGuid(ErrorMessage = "Learner ID is required.")]
    public Guid LearnerId { get; init; }
}