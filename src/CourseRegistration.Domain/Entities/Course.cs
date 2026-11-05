namespace CourseRegistration.Domain.Entities;

public sealed class Course
{
    public Guid Id { get; }
    public string Name { get; }

    public Course(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Course ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name is required.");

        Id = id;
        Name = name.Trim();
    }
}