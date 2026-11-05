
namespace CourseRegistration.Application.Interfaces;

public interface IPrerequisiteEligibilityService
{
    bool IsEligible(Guid learnerId, Guid courseId);
}
