
using CourseRegistration.Domain.Entities;

namespace CourseRegistration.Application.Interfaces;

public interface ICourseRegistrationRepository
{
    Course? GetCourse(Guid courseId);

    CourseOffering? GetOffering(Guid offeringId);

    Learner? GetLearner(Guid learnerId);

    IReadOnlyCollection<CoursePrerequisite> GetPrerequisites(Guid courseId);

    IReadOnlyCollection<LearnerCompletedCourse> GetCompletedCourses(
        Guid learnerId);

    Enrollment? GetEnrollment(
        Guid offeringId,
        Guid learnerId);

    IReadOnlyCollection<Enrollment> GetEnrollments(Guid offeringId);

    WaitlistEntry? GetWaitlistEntry(
        Guid offeringId,
        Guid learnerId);

    IReadOnlyCollection<WaitlistEntry> GetWaitlist(Guid offeringId);

    void AddEnrollment(Enrollment enrollment);

    void RemoveEnrollment(Enrollment enrollment);

    void AddWaitlistEntry(WaitlistEntry entry);

    void RemoveWaitlistEntry(WaitlistEntry entry);

    long GetNextQueueNumber(Guid offeringId);

    void RemoveCompletedCourse(Guid learnerId, Guid courseId);
    
    void CompleteDropAndPromotion(
    Enrollment enrollment,
    WaitlistEntry? promotedEntry,
    Enrollment? promotedEnrollment);
}
