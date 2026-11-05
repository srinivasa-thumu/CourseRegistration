namespace CourseRegistration.Domain.Entities;

public sealed class Enrollment
{
    public Guid Id { get; }
    public Guid CourseOfferingId { get; }
    public Guid LearnerId { get; }
    public DateTimeOffset EnrolledAt { get; }

    public Enrollment(
        Guid id,
        Guid courseOfferingId,
        Guid learnerId,
        DateTimeOffset enrolledAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Enrollment ID cannot be empty.");

        if (courseOfferingId == Guid.Empty)
            throw new ArgumentException("Offering ID cannot be empty.");

        if (learnerId == Guid.Empty)
            throw new ArgumentException("Learner ID cannot be empty.");

        if (enrolledAt == default)
            throw new ArgumentException("Enrollment date is required.");

        Id = id;
        CourseOfferingId = courseOfferingId;
        LearnerId = learnerId;
        EnrolledAt = enrolledAt;
    }
}