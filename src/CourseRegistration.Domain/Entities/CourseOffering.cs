namespace CourseRegistration.Domain.Entities;

public sealed class CourseOffering
{
    public Guid Id { get; }
    public Guid CourseId { get; }
    public int Capacity { get; }

    public CourseOffering(
        Guid id,
        Guid courseId,
        int capacity)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Offering ID cannot be empty.");

        if (courseId == Guid.Empty)
            throw new ArgumentException("Course ID cannot be empty.");

        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.");

        Id = id;
        CourseId = courseId;
        Capacity = capacity;
    }
}