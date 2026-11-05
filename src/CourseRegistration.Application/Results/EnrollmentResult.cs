
namespace CourseRegistration.Application.Results;

public sealed record EnrollmentResult(
    bool Success,
    string Code,
    string Message,
    Guid? EnrollmentId = null,
    Guid? WaitlistEntryId = null);
