
using CourseRegistration.Application.Services;
using CourseRegistration.Infrastructure.Repositories;
using Xunit;

namespace CourseRegistration.Tests;

public sealed class EnrollmentServiceTests
{
    private static readonly Guid OfferingId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    private static readonly Guid Alice =
        Guid.Parse("30000000-0000-0000-0000-000000000001");

    private static readonly Guid Bob =
        Guid.Parse("30000000-0000-0000-0000-000000000002");

    private static readonly Guid Charlie =
        Guid.Parse("30000000-0000-0000-0000-000000000003");

    private static readonly Guid David =
        Guid.Parse("30000000-0000-0000-0000-000000000004");

    private static readonly Guid Emma =
        Guid.Parse("30000000-0000-0000-0000-000000000005");

    private static readonly Guid Frank =
        Guid.Parse("30000000-0000-0000-0000-000000000006");

    private static (
        InMemoryCourseRegistrationRepository Repository,
        EnrollmentService Service) CreateSut()
    {
        var repository = new InMemoryCourseRegistrationRepository();

        var eligibilityService =
            new PrerequisiteEligibilityService(repository);

        var service = new EnrollmentService(
            repository,
            eligibilityService);

        return (repository, service);
    }

    private static async Task FillCourseAsync(
        EnrollmentService service)
    {
        await service.EnrollAsync(OfferingId, Alice);
        await service.EnrollAsync(OfferingId, Bob);
        await service.EnrollAsync(OfferingId, Charlie);
    }

    [Fact]
    public async Task EnrollAsync_WhenSeatAvailable_ShouldEnrollLearner()
    {
        var (repository, service) = CreateSut();

        var result = await service.EnrollAsync(OfferingId, Alice);

        Assert.True(result.Success);
        Assert.Equal("ENROLLED", result.Code);
        Assert.Single(repository.GetEnrollments(OfferingId));
    }

    [Fact]
    public async Task EnrollAsync_WhenCourseFull_ShouldAddLearnerToWaitlist()
    {
        var (repository, service) = CreateSut();

        await FillCourseAsync(service);

        var result = await service.EnrollAsync(OfferingId, David);

        Assert.True(result.Success);
        Assert.Equal("WAITLISTED", result.Code);
        Assert.Equal(3, repository.GetEnrollments(OfferingId).Count);
        Assert.Single(repository.GetWaitlist(OfferingId));
    }

    [Fact]
    public async Task EnrollAsync_WhenLearnerAlreadyEnrolled_ShouldRejectDuplicate()
    {
        var (repository, service) = CreateSut();

        await service.EnrollAsync(OfferingId, Alice);

        var result = await service.EnrollAsync(OfferingId, Alice);

        Assert.False(result.Success);
        Assert.Equal("DUPLICATE_REQUEST", result.Code);
        Assert.Single(repository.GetEnrollments(OfferingId));
    }

    [Fact]
    public async Task EnrollAsync_WhenLearnerAlreadyWaitlisted_ShouldRejectDuplicate()
    {
        var (repository, service) = CreateSut();

        await FillCourseAsync(service);

        await service.EnrollAsync(OfferingId, David);

        var result = await service.EnrollAsync(OfferingId, David);

        Assert.False(result.Success);
        Assert.Equal("DUPLICATE_REQUEST", result.Code);
        Assert.Single(repository.GetWaitlist(OfferingId));
    }

    [Fact]
    public async Task EnrollAsync_WhenPrerequisiteMissing_ShouldRejectLearner()
    {
        var (repository, service) = CreateSut();

        var result = await service.EnrollAsync(OfferingId, Frank);

        Assert.False(result.Success);
        Assert.Equal("PREREQUISITE_FAILED", result.Code);
        Assert.Empty(repository.GetEnrollments(OfferingId));
        Assert.Empty(repository.GetWaitlist(OfferingId));
    }

    [Fact]
    public async Task DropAsync_WhenLearnerEnrolled_ShouldPromoteFirstWaitlistedLearner()
    {
        var (repository, service) = CreateSut();

        await FillCourseAsync(service);
        await service.EnrollAsync(OfferingId, David);
        await service.EnrollAsync(OfferingId, Emma);

        var result = await service.DropAsync(OfferingId, Bob);

        Assert.True(result.Success);
        Assert.Equal("DROPPED", result.Code);
        Assert.Equal(David, result.PromotedLearnerId);

        Assert.Equal(3, repository.GetEnrollments(OfferingId).Count);
        Assert.Null(repository.GetEnrollment(OfferingId, Bob));
        Assert.NotNull(repository.GetEnrollment(OfferingId, David));
        Assert.Null(repository.GetWaitlistEntry(OfferingId, David));
        Assert.NotNull(repository.GetWaitlistEntry(OfferingId, Emma));
    }

    [Fact]
    public async Task DropAsync_WhenFirstWaitlistedLearnerBecomesIneligible_ShouldSkipAndRetainThem()
    {
        var (repository, service) = CreateSut();

        await FillCourseAsync(service);
        await service.EnrollAsync(OfferingId, David);
        await service.EnrollAsync(OfferingId, Emma);

        var distributedSystemsId =
            repository.GetOffering(OfferingId)!.CourseId;

        var operatingSystemsId =
            Guid.Parse("10000000-0000-0000-0000-000000000002");

        // David was eligible when joining but is now ineligible.
        repository.RemoveCompletedCourse(David, operatingSystemsId);

        var result = await service.DropAsync(OfferingId, Bob);

        Assert.True(result.Success);
        Assert.Equal(Emma, result.PromotedLearnerId);

        // David remains in the waitlist.
        var remainingWaitlist = repository.GetWaitlist(OfferingId);

        var davidEntry = Assert.Single(remainingWaitlist);

        Assert.Equal(David, davidEntry.LearnerId);
        Assert.Equal(1, davidEntry.QueueNumber);

        Assert.NotNull(repository.GetEnrollment(OfferingId, Emma));
        Assert.Null(repository.GetEnrollment(OfferingId, David));
    }

    [Fact]
    public async Task DropAsync_WhenLearnerNotEnrolled_ShouldNotModifyState()
    {
        var (repository, service) = CreateSut();

        await service.EnrollAsync(OfferingId, Alice);

        var result = await service.DropAsync(OfferingId, Bob);

        Assert.False(result.Success);
        Assert.Equal("NOT_ENROLLED", result.Code);
        Assert.Single(repository.GetEnrollments(OfferingId));
    }

    [Fact]
    public async Task DropAsync_WhenLearnerDropsTwice_ShouldRejectSecondDrop()
    {
        var (repository, service) = CreateSut();

        await service.EnrollAsync(OfferingId, Alice);

        var firstResult = await service.DropAsync(OfferingId, Alice);
        var secondResult = await service.DropAsync(OfferingId, Alice);

        Assert.True(firstResult.Success);
        Assert.False(secondResult.Success);
        Assert.Equal("NOT_ENROLLED", secondResult.Code);
        Assert.Empty(repository.GetEnrollments(OfferingId));
    }

    [Fact]
    public async Task EnrollAsync_WhenMultipleRequestsArriveConcurrently_ShouldPreserveCapacity()
    {
        var (repository, service) = CreateSut();

        var learners = new[]
        {
            Alice, Bob, Charlie, David, Emma
        };

        var tasks = learners.Select(
            learnerId => service.EnrollAsync(OfferingId, learnerId));

        var results = await Task.WhenAll(tasks);

        Assert.All(results, result => Assert.True(result.Success));

        Assert.Equal(3, repository.GetEnrollments(OfferingId).Count);
        Assert.Equal(2, repository.GetWaitlist(OfferingId).Count);

        Assert.Equal(
            repository.GetWaitlist(OfferingId).Count,
            repository.GetWaitlist(OfferingId)
                .Select(x => x.LearnerId)
                .Distinct()
                .Count());
    }

    [Fact]
    public async Task EnrollAsync_WhenOfferingDoesNotExist_ShouldReturnNotFound()
    {
        var (_, service) = CreateSut();

        var result = await service.EnrollAsync(
            Guid.NewGuid(), Alice);

        Assert.False(result.Success);
        Assert.Equal("COURSE_NOT_FOUND", result.Code);
    }

    [Fact]
    public void Eligibility_WhenCourseHasNoPrerequisites_ShouldAllowEnrollment()
    {
        // Arrange
        var repository = new InMemoryCourseRegistrationRepository();

        var eligibilityService =
            new PrerequisiteEligibilityService(repository);

        var dataStructuresCourseId =
            Guid.Parse("10000000-0000-0000-0000-000000000001");

        // Alice is an existing seeded learner.
        var learnerId = Alice;

        // Act
        var isEligible = eligibilityService.IsEligible(
            learnerId,
            dataStructuresCourseId);

        // Assert
        Assert.True(isEligible);
    }

        [Fact]
    public async Task CompleteDropAndPromotion_WhenPromotionEntryIsInvalid_ShouldPreserveExistingEnrollment()
    {
        var (repository, service) = CreateSut();

        await service.EnrollAsync(OfferingId, Alice);

        var originalEnrollment =
            repository.GetEnrollment(OfferingId, Alice)!;

        var invalidWaitlistEntry = new CourseRegistration.Domain.Entities.WaitlistEntry(
            Guid.NewGuid(),
            OfferingId,
            David,
            1,
            DateTimeOffset.UtcNow);

        var invalidPromotedEnrollment =
            new CourseRegistration.Domain.Entities.Enrollment(
                Guid.NewGuid(),
                OfferingId,
                Emma,
                DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            repository.CompleteDropAndPromotion(
                originalEnrollment,
                invalidWaitlistEntry,
                invalidPromotedEnrollment));

        // Original enrollment must remain untouched.
        Assert.NotNull(repository.GetEnrollment(OfferingId, Alice));
        Assert.Single(repository.GetEnrollments(OfferingId));
    }

    [Fact]
    public async Task EnrollAsync_WhenLearnerDoesNotExist_ShouldReturnNotFound()
    {
        var (_, service) = CreateSut();

        var result = await service.EnrollAsync(
            OfferingId, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("LEARNER_NOT_FOUND", result.Code);
    }

    [Fact]
    public async Task EnrollAsync_WhenIdentifiersAreEmpty_ShouldReturnInvalidRequest()
    {
        var (_, service) = CreateSut();

        var result = await service.EnrollAsync(
            Guid.Empty, Guid.Empty);

        Assert.False(result.Success);
        Assert.Equal("INVALID_REQUEST", result.Code);
    }

    
    [Fact]
    public async Task DropAsync_WhenMultipleLearnersDropConcurrently_ShouldPromoteDifferentLearners()
    {
        var (repository, service) = CreateSut();

        await FillCourseAsync(service);
        await service.EnrollAsync(OfferingId, David);
        await service.EnrollAsync(OfferingId, Emma);

        var results = await Task.WhenAll(
            service.DropAsync(OfferingId, Alice),
            service.DropAsync(OfferingId, Bob));

        Assert.All(results, result => Assert.True(result.Success));

        // Charlie remains enrolled.
        // David and Emma should both be promoted.
        Assert.Equal(3, repository.GetEnrollments(OfferingId).Count);
        Assert.Empty(repository.GetWaitlist(OfferingId));

        Assert.NotNull(repository.GetEnrollment(OfferingId, David));
        Assert.NotNull(repository.GetEnrollment(OfferingId, Emma));
        Assert.NotNull(repository.GetEnrollment(OfferingId, Charlie));
    }

}
