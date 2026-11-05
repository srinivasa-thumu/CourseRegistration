
using CourseRegistration.Application.Interfaces;
using CourseRegistration.Application.Results;
using CourseRegistration.Domain.Entities;

namespace CourseRegistration.Application.Services;

public sealed class EnrollmentService
{
    private readonly ICourseRegistrationRepository _repository;
    private readonly IPrerequisiteEligibilityService _eligibilityService;

    // Protects state-changing operations within this application process.
    private readonly SemaphoreSlim _gate = new(1, 1);

    public EnrollmentService(
        ICourseRegistrationRepository repository,
        IPrerequisiteEligibilityService eligibilityService)
    {
        _repository = repository;
        _eligibilityService = eligibilityService;
    }

    public async Task<EnrollmentResult> EnrollAsync(
        Guid offeringId,
        Guid learnerId,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (offeringId == Guid.Empty || learnerId == Guid.Empty)
            {
                return new EnrollmentResult(
                    false,
                    "INVALID_REQUEST",
                    "Offering ID and learner ID are required.");
            }

            var offering = _repository.GetOffering(offeringId);

            if (offering is null)
            {
                return new EnrollmentResult(
                    false,
                    "COURSE_NOT_FOUND",
                    "Course offering was not found.");
            }

            if (_repository.GetLearner(learnerId) is null)
            {
                return new EnrollmentResult(
                    false,
                    "LEARNER_NOT_FOUND",
                    "Learner was not found.");
            }

            // Prevent duplicate active enrollment and waitlist entries.
            if (_repository.GetEnrollment(offeringId, learnerId) is not null ||
                _repository.GetWaitlistEntry(offeringId, learnerId) is not null)
            {
                return new EnrollmentResult(
                    false,
                    "DUPLICATE_REQUEST",
                    "Learner is already enrolled or waitlisted.");
            }

            // Prerequisites must be satisfied before enrollment
            // or joining the waitlist.
            if (!_eligibilityService.IsEligible(
                    learnerId, offering.CourseId))
            {
                return new EnrollmentResult(
                    false,
                    "PREREQUISITE_FAILED",
                    "Learner has not completed all required prerequisites.");
            }

            var enrolledCount =
                _repository.GetEnrollments(offeringId).Count;

            // Seat available.
            if (enrolledCount < offering.Capacity)
            {
                var enrollment = new Enrollment(
                    Guid.NewGuid(),
                    offeringId,
                    learnerId,
                    DateTimeOffset.UtcNow);

                _repository.AddEnrollment(enrollment);

                return new EnrollmentResult(
                    true,
                    "ENROLLED",
                    "Learner enrolled successfully.",
                    EnrollmentId: enrollment.Id);
            }

            // Course full: add learner to waitlist.
            var waitlistEntry = new WaitlistEntry(
                Guid.NewGuid(),
                offeringId,
                learnerId,
                _repository.GetNextQueueNumber(offeringId),
                DateTimeOffset.UtcNow);

            _repository.AddWaitlistEntry(waitlistEntry);

            return new EnrollmentResult(
                true,
                "WAITLISTED",
                "Course is full. Learner added to the waitlist.",
                WaitlistEntryId: waitlistEntry.Id);
        }
        finally
        {
            _gate.Release();
        }
    }

    
    public async Task<DropResult> DropAsync(
        Guid offeringId,
        Guid learnerId,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (offeringId == Guid.Empty || learnerId == Guid.Empty)
            {
                return new DropResult(
                    false,
                    "INVALID_REQUEST",
                    "Offering ID and learner ID are required.");
            }

            var offering = _repository.GetOffering(offeringId);

            if (offering is null)
            {
                return new DropResult(
                    false,
                    "COURSE_NOT_FOUND",
                    "Course offering was not found.");
            }

            var enrollment =
                _repository.GetEnrollment(offeringId, learnerId);

            if (enrollment is null)
            {
                return new DropResult(
                    false,
                    "NOT_ENROLLED",
                    "Learner does not have an active enrollment.");
            }

            // Find the earliest eligible learner without modifying state.
            var waitlist = _repository
                .GetWaitlist(offeringId)
                .OrderBy(x => x.QueueNumber);

            WaitlistEntry? selectedEntry = null;

            foreach (var entry in waitlist)
            {
                if (!_eligibilityService.IsEligible(
                        entry.LearnerId,
                        offering.CourseId))
                {
                    // Skip but retain the ineligible learner.
                    continue;
                }

                selectedEntry = entry;
                break;
            }

            Enrollment? promotedEnrollment = null;

            if (selectedEntry is not null)
            {
                promotedEnrollment = new Enrollment(
                    Guid.NewGuid(),
                    offeringId,
                    selectedEntry.LearnerId,
                    DateTimeOffset.UtcNow);
            }

            // One atomic repository operation.
            _repository.CompleteDropAndPromotion(
                enrollment,
                selectedEntry,
                promotedEnrollment);

            if (selectedEntry is not null)
            {
                return new DropResult(
                    true,
                    "DROPPED",
                    "Learner dropped successfully. " +
                    "The earliest eligible waitlisted learner was promoted.",
                    selectedEntry.LearnerId);
            }

            return new DropResult(
                true,
                "DROPPED",
                "Learner dropped successfully. No eligible waitlisted learner was available.");
        }
        finally
        {
            _gate.Release();
        }
    }
}
