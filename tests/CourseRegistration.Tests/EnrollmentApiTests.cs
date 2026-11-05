
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace CourseRegistration.Tests;

public sealed class EnrollmentApiTests
{
    private const string OfferingId =
        "20000000-0000-0000-0000-000000000001";

    private static string LearnerId(int number) =>
        $"30000000-0000-0000-0000-00000000000{number}";

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            });
    }

    private static async Task<HttpResponseMessage> EnrollAsync(
        HttpClient client,
        int learnerNumber)
    {
        return await client.PostAsJsonAsync(
            "/api/enrollments",
            new
            {
                CourseOfferingId = OfferingId,
                LearnerId = LearnerId(learnerNumber)
            });
    }

    [Fact]
    public async Task Enroll_WhenSeatAvailable_ShouldReturnCreated()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await EnrollAsync(client, 1);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    
    [Fact]
    public async Task Enroll_WhenOfferingIdIsEmpty_ShouldReturnBadRequest()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/enrollments",
            new
            {
                CourseOfferingId = Guid.Empty,
                LearnerId = LearnerId(1)
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task Enroll_WhenCourseFull_ShouldReturnAccepted()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        await EnrollAsync(client, 1);
        await EnrollAsync(client, 2);
        await EnrollAsync(client, 3);

        var response = await EnrollAsync(client, 4);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Enroll_WhenDuplicate_ShouldReturnConflict()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        await EnrollAsync(client, 1);

        var response = await EnrollAsync(client, 1);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Enroll_WhenPrerequisiteMissing_ShouldReturnUnprocessableEntity()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await EnrollAsync(client, 6);

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            response.StatusCode);
    }

    [Fact]
    public async Task Drop_WhenLearnerEnrolled_ShouldReturnSuccess()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        await EnrollAsync(client, 1);

        var response = await client.DeleteAsync(
            $"/api/enrollments/{OfferingId}/{LearnerId(1)}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Drop_WhenLearnerNotEnrolled_ShouldReturnConflict()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/enrollments/{OfferingId}/{LearnerId(1)}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetCourseStatus_ShouldReturnCapacityInformation()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        await EnrollAsync(client, 1);

        var response = await client.GetAsync(
            $"/api/enrollments/course/{OfferingId}/status");

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<CourseStatusTestResponse>();

        Assert.NotNull(status);
        Assert.Equal(3, status.Capacity);
        Assert.Equal(1, status.EnrolledCount);
        Assert.Equal(2, status.AvailableSeats);
    }

    [Fact]
    public async Task Swagger_ShouldBeAvailable()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record CourseStatusTestResponse(
        Guid CourseOfferingId,
        Guid CourseId,
        int Capacity,
        int EnrolledCount,
        int AvailableSeats,
        int WaitlistCount);
}
