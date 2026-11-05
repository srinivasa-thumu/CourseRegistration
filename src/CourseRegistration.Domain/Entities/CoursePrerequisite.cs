namespace CourseRegistration.Domain.Entities;

public sealed class CoursePrerequisite
{
    public Guid CourseId { get; }
    public Guid PrerequisiteCourseId { get; }

    public CoursePrerequisite(
        Guid courseId,
        Guid prerequisiteCourseId)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("Course ID cannot be empty.");

        if (prerequisiteCourseId == Guid.Empty)
            throw new ArgumentException("Prerequisite course ID cannot be empty.");

        if (courseId == prerequisiteCourseId)
            throw new ArgumentException(
                "A course cannot be its own prerequisite.");

        CourseId = courseId;
        PrerequisiteCourseId = prerequisiteCourseId;
    }
}