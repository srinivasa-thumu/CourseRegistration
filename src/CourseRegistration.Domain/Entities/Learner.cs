namespace CourseRegistration.Domain.Entities;

public sealed class Learner
{
    public Guid Id { get; }
    public string Name { get; }

    public Learner(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Learner ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Learner name is required.");

        Id = id;
        Name = name.Trim();
    }
}