
using CourseRegistration.Application.Interfaces;
using CourseRegistration.Application.Services;
using CourseRegistration.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Shared repository and service instances are essential
// for our single-process synchronization strategy.
builder.Services.AddSingleton<
    ICourseRegistrationRepository,
    InMemoryCourseRegistrationRepository>();

builder.Services.AddSingleton<
    IPrerequisiteEligibilityService,
    PrerequisiteEligibilityService>();

builder.Services.AddSingleton<EnrollmentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();

// Required for WebApplicationFactory integration tests.
public partial class Program{
    
}
