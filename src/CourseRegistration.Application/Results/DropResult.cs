
namespace CourseRegistration.Application.Results;

public sealed record DropResult(
    bool Success,
    string Code,
    string Message,
    Guid? PromotedLearnerId = null);
