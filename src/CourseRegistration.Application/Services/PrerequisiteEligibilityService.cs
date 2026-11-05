
using CourseRegistration.Application.Interfaces;

namespace CourseRegistration.Application.Services;

public sealed class PrerequisiteEligibilityService
    : IPrerequisiteEligibilityService
{
    private readonly ICourseRegistrationRepository _repository;

    public PrerequisiteEligibilityService(
        ICourseRegistrationRepository repository)
    {
        _repository = repository;
    }

    public bool IsEligible(Guid learnerId, Guid courseId)
    {
        var prerequisites = _repository.GetPrerequisites(courseId);

        // No prerequisites means the learner is eligible.
        if (prerequisites.Count == 0)
            return true;

        var completedCourseIds = _repository
            .GetCompletedCourses(learnerId)
            .Select(x => x.CourseId)
            .ToHashSet();

        // Every configured prerequisite must be completed.
        return prerequisites.All(
            prerequisite =>
                completedCourseIds.Contains(
                    prerequisite.PrerequisiteCourseId));
    }
}
