namespace CourseRegistration.Domain.Entities;

public sealed class WaitlistEntry
{
    public Guid Id { get; }
    public Guid CourseOfferingId { get; }
    public Guid LearnerId { get; }
    public long QueueNumber { get; }
    public DateTimeOffset CreatedAt { get; }

    public WaitlistEntry(
        Guid id,
        Guid courseOfferingId,
        Guid learnerId,
        long queueNumber,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Waitlist ID cannot be empty.");

        if (courseOfferingId == Guid.Empty)
            throw new ArgumentException("Offering ID cannot be empty.");

        if (learnerId == Guid.Empty)
            throw new ArgumentException("Learner ID cannot be empty.");

        if (queueNumber <= 0)
            throw new ArgumentException("Queue number must be positive.");

        if (createdAt == default)
            throw new ArgumentException("Creation date is required.");

        Id = id;
        CourseOfferingId = courseOfferingId;
        LearnerId = learnerId;
        QueueNumber = queueNumber;
        CreatedAt = createdAt;
    }
}