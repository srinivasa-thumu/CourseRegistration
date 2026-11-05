
using CourseRegistration.Api.Contracts;
using CourseRegistration.Application.Interfaces;
using CourseRegistration.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseRegistration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EnrollmentsController : ControllerBase
{
    private readonly EnrollmentService _enrollmentService;
    private readonly ICourseRegistrationRepository _repository;

    public EnrollmentsController(
        EnrollmentService enrollmentService,
        ICourseRegistrationRepository repository)
    {
        _enrollmentService = enrollmentService;
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(
        [FromBody] EnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.EnrollAsync(
            request.CourseOfferingId,
            request.LearnerId,
            cancellationToken);

        if (!result.Success)
        {
            return result.Code switch
            {
                "INVALID_REQUEST" => BadRequest(result),
                "COURSE_NOT_FOUND" => NotFound(result),
                "LEARNER_NOT_FOUND" => NotFound(result),
                "PREREQUISITE_FAILED" => UnprocessableEntity(result),
                _ => Conflict(result)
            };
        }

        return result.Code switch
        {
            "ENROLLED" => StatusCode(201, result),
            "WAITLISTED" => Accepted(result),
            _ => Ok(result)
        };
    }

    [HttpDelete("{offeringId:guid}/{learnerId:guid}")]
    public async Task<IActionResult> Drop(
        Guid offeringId,
        Guid learnerId,
        CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.DropAsync(
            offeringId,
            learnerId,
            cancellationToken);

        if (!result.Success)
        {
            return result.Code switch
            {
                "INVALID_REQUEST" => BadRequest(result),
                "COURSE_NOT_FOUND" => NotFound(result),
                _ => Conflict(result)
            };
        }

        return Ok(result);
    }

    [HttpGet("course/{offeringId:guid}/status")]
    public IActionResult GetCourseStatus(Guid offeringId)
    {
        var offering = _repository.GetOffering(offeringId);

        if (offering is null)
            return NotFound(new { Message = "Course offering not found." });

        var enrolledCount =
            _repository.GetEnrollments(offeringId).Count;

        var waitlistCount =
            _repository.GetWaitlist(offeringId).Count;

        return Ok(new CourseStatusResponse(
            offering.Id,
            offering.CourseId,
            offering.Capacity,
            enrolledCount,
            offering.Capacity - enrolledCount,
            waitlistCount));
    }

    [HttpGet("course/{offeringId:guid}/enrolled")]
    public IActionResult GetEnrolledLearners(Guid offeringId)
    {
        if (_repository.GetOffering(offeringId) is null)
            return NotFound();

        var enrollments = _repository.GetEnrollments(offeringId);

        var result = enrollments.Select(enrollment => new
        {
            enrollment.LearnerId,
            LearnerName = _repository.GetLearner(
                enrollment.LearnerId)?.Name,
            enrollment.EnrolledAt
        });

        return Ok(result);
    }

    [HttpGet("course/{offeringId:guid}/waitlist")]
    public IActionResult GetWaitlist(Guid offeringId)
    {
        if (_repository.GetOffering(offeringId) is null)
            return NotFound();

        var entries = _repository.GetWaitlist(offeringId);

        var result = entries.Select(entry => new
        {
            entry.LearnerId,
            LearnerName = _repository.GetLearner(
                entry.LearnerId)?.Name,
            entry.QueueNumber,
            entry.CreatedAt
        });

        return Ok(result);
    }
}
