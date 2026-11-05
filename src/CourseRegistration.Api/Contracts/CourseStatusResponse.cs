
namespace CourseRegistration.Api.Contracts;

public sealed record CourseStatusResponse(
    Guid CourseOfferingId,
    Guid CourseId,
    int Capacity,
    int EnrolledCount,
    int AvailableSeats,
    int WaitlistCount);
