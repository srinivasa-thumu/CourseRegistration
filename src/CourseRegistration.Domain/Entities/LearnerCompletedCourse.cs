namespace CourseRegistration.Domain.Entities;

public sealed class LearnerCompletedCourse
{
    public Guid LearnerId { get; }
    public Guid CourseId { get; }
    public DateTimeOffset CompletedAt { get; }

    public LearnerCompletedCourse(
        Guid learnerId,
        Guid courseId,
        DateTimeOffset completedAt)
    {
        if (learnerId == Guid.Empty)
            throw new ArgumentException("Learner ID cannot be empty.");

        if (courseId == Guid.Empty)
            throw new ArgumentException("Course ID cannot be empty.");

        if (completedAt == default)
            throw new ArgumentException("Completion date is required.");

        CompletedAt = completedAt;
        LearnerId = learnerId;
        CourseId = courseId;
    }
}