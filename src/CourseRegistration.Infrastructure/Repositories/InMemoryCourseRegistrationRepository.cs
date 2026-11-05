
using CourseRegistration.Application.Interfaces;
using CourseRegistration.Domain.Entities;

namespace CourseRegistration.Infrastructure.Repositories;

public sealed class InMemoryCourseRegistrationRepository
    : ICourseRegistrationRepository
{
    private readonly object _sync = new();

    private readonly Dictionary<Guid, Course> _courses = new();
    private readonly Dictionary<Guid, CourseOffering> _offerings = new();
    private readonly Dictionary<Guid, Learner> _learners = new();

    private readonly List<CoursePrerequisite> _prerequisites = [];
    private readonly List<LearnerCompletedCourse> _completedCourses = [];
    private readonly List<Enrollment> _enrollments = [];
    private readonly List<WaitlistEntry> _waitlist = [];

    private readonly Dictionary<Guid, long> _queueCounters = new();

    public InMemoryCourseRegistrationRepository()
    {
        SeedData();
    }

    private void SeedData()
    {
        var dataStructuresId =
            Guid.Parse("10000000-0000-0000-0000-000000000001");

        var operatingSystemsId =
            Guid.Parse("10000000-0000-0000-0000-000000000002");

        var distributedSystemsId =
            Guid.Parse("10000000-0000-0000-0000-000000000003");

        var offeringId =
            Guid.Parse("20000000-0000-0000-0000-000000000001");

        _courses[dataStructuresId] =
            new Course(dataStructuresId, "Data Structures");

        _courses[operatingSystemsId] =
            new Course(operatingSystemsId, "Operating Systems");

        _courses[distributedSystemsId] =
            new Course(distributedSystemsId, "Distributed Systems");

        _offerings[offeringId] =
            new CourseOffering(offeringId, distributedSystemsId, 3);

        var learnerA = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var learnerB = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var learnerC = Guid.Parse("30000000-0000-0000-0000-000000000003");
        var learnerD = Guid.Parse("30000000-0000-0000-0000-000000000004");
        var learnerE = Guid.Parse("30000000-0000-0000-0000-000000000005");
        var learnerF = Guid.Parse("30000000-0000-0000-0000-000000000006");

        _learners[learnerA] = new Learner(learnerA, "Alice");
        _learners[learnerB] = new Learner(learnerB, "Bob");
        _learners[learnerC] = new Learner(learnerC, "Charlie");
        _learners[learnerD] = new Learner(learnerD, "David");
        _learners[learnerE] = new Learner(learnerE, "Emma");
        _learners[learnerF] = new Learner(learnerF, "Frank");

        // Distributed Systems requires both prerequisites.
        _prerequisites.AddRange(
        [
            new CoursePrerequisite(
                distributedSystemsId, dataStructuresId),

            new CoursePrerequisite(
                distributedSystemsId, operatingSystemsId)
        ]);

        // Alice, Bob, Charlie, David and Emma satisfy prerequisites.
        foreach (var learnerId in new[]
        {
            learnerA, learnerB, learnerC, learnerD, learnerE
        })
        {
            _completedCourses.Add(
                new LearnerCompletedCourse(
                    learnerId,
                    dataStructuresId,
                    DateTimeOffset.UtcNow));

            _completedCourses.Add(
                new LearnerCompletedCourse(
                    learnerId,
                    operatingSystemsId,
                    DateTimeOffset.UtcNow));
        }

        // Frank has completed only Data Structures.
        _completedCourses.Add(
            new LearnerCompletedCourse(
                learnerF,
                dataStructuresId,
                DateTimeOffset.UtcNow));

        _queueCounters[offeringId] = 0;
    }

    public Course? GetCourse(Guid courseId)
    {
        lock (_sync)
            return _courses.GetValueOrDefault(courseId);
    }

    public CourseOffering? GetOffering(Guid offeringId)
    {
        lock (_sync)
            return _offerings.GetValueOrDefault(offeringId);
    }

    public Learner? GetLearner(Guid learnerId)
    {
        lock (_sync)
            return _learners.GetValueOrDefault(learnerId);
    }

    public IReadOnlyCollection<CoursePrerequisite> GetPrerequisites(
        Guid courseId)
    {
        lock (_sync)
            return _prerequisites
                .Where(x => x.CourseId == courseId)
                .ToArray();
    }

    public IReadOnlyCollection<LearnerCompletedCourse> GetCompletedCourses(
        Guid learnerId)
    {
        lock (_sync)
            return _completedCourses
                .Where(x => x.LearnerId == learnerId)
                .ToArray();
    }

    public Enrollment? GetEnrollment(
        Guid offeringId, Guid learnerId)
    {
        lock (_sync)
            return _enrollments.FirstOrDefault(
                x => x.CourseOfferingId == offeringId &&
                     x.LearnerId == learnerId);
    }

    public IReadOnlyCollection<Enrollment> GetEnrollments(Guid offeringId)
    {
        lock (_sync)
            return _enrollments
                .Where(x => x.CourseOfferingId == offeringId)
                .ToArray();
    }

    public WaitlistEntry? GetWaitlistEntry(
        Guid offeringId, Guid learnerId)
    {
        lock (_sync)
            return _waitlist.FirstOrDefault(
                x => x.CourseOfferingId == offeringId &&
                     x.LearnerId == learnerId);
    }

    public IReadOnlyCollection<WaitlistEntry> GetWaitlist(Guid offeringId)
    {
        lock (_sync)
            return _waitlist
                .Where(x => x.CourseOfferingId == offeringId)
                .OrderBy(x => x.QueueNumber)
                .ToArray();
    }

    public void AddEnrollment(Enrollment enrollment)
    {
        lock (_sync)
        {
            if (_enrollments.Any(x =>
                x.CourseOfferingId == enrollment.CourseOfferingId &&
                x.LearnerId == enrollment.LearnerId))
            {
                throw new InvalidOperationException(
                    "Duplicate enrollment.");
            }

            _enrollments.Add(enrollment);
        }
    }

    public void RemoveEnrollment(Enrollment enrollment)
    {
        lock (_sync)
            _enrollments.Remove(enrollment);
    }

    public void AddWaitlistEntry(WaitlistEntry entry)
    {
        lock (_sync)
        {
            if (_waitlist.Any(x =>
                x.CourseOfferingId == entry.CourseOfferingId &&
                x.LearnerId == entry.LearnerId))
            {
                throw new InvalidOperationException(
                    "Duplicate waitlist entry.");
            }

            _waitlist.Add(entry);
        }
    }

    public void RemoveWaitlistEntry(WaitlistEntry entry)
    {
        lock (_sync)
            _waitlist.Remove(entry);
    }

    public long GetNextQueueNumber(Guid offeringId)
    {
        lock (_sync)
        {
            var next = _queueCounters.GetValueOrDefault(offeringId) + 1;
            _queueCounters[offeringId] = next;
            return next;
        }
    }

    public void RemoveCompletedCourse(Guid learnerId, Guid courseId)
    {
        lock (_sync)
        {
            _completedCourses.RemoveAll(x =>
                x.LearnerId == learnerId &&
                x.CourseId == courseId);
        }
    }

     public void CompleteDropAndPromotion(
    Enrollment enrollment,
    WaitlistEntry? promotedEntry,
    Enrollment? promotedEnrollment)
    {
        lock (_sync)
        {
            var existingEnrollment = _enrollments.FirstOrDefault(x =>
                x.Id == enrollment.Id);

            if (existingEnrollment is null)
                throw new InvalidOperationException(
                    "Enrollment no longer exists.");

            // Validate the complete transition before changing state.
            if ((promotedEntry is null) != (promotedEnrollment is null))
                throw new InvalidOperationException(
                    "Promotion entry and enrollment must be supplied together.");

            if (promotedEntry is not null && promotedEnrollment is not null)
            {
                var existingWaitlistEntry = _waitlist.FirstOrDefault(x =>
                    x.Id == promotedEntry.Id &&
                    x.CourseOfferingId == enrollment.CourseOfferingId);

                if (existingWaitlistEntry is null)
                    throw new InvalidOperationException(
                        "Waitlist entry no longer exists.");

                if (promotedEnrollment.CourseOfferingId !=
                        enrollment.CourseOfferingId ||
                    promotedEnrollment.LearnerId != promotedEntry.LearnerId)
                {
                    throw new InvalidOperationException(
                        "Promotion enrollment does not match waitlist entry.");
                }

                if (_enrollments.Any(x =>
                    x.CourseOfferingId == promotedEnrollment.CourseOfferingId &&
                    x.LearnerId == promotedEnrollment.LearnerId))
                {
                    throw new InvalidOperationException(
                        "Learner is already enrolled.");
                }
            }

            // All validations passed. Apply the state transition.
            _enrollments.Remove(existingEnrollment);

            if (promotedEntry is not null &&
                promotedEnrollment is not null)
            {
                _waitlist.Remove(promotedEntry);
                _enrollments.Add(promotedEnrollment);
            }
        }
    }
}
