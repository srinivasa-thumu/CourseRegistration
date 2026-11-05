# Course Registration System

A .NET 8 Web API implementing course offering enrollment, capacity validation, prerequisite enforcement, FIFO waitlisting and automatic waitlist promotion when an enrolled learner drops.

## Features

- Enroll learners into course offerings.
- Enforce offering capacity.
- Validate course prerequisites.
- Prevent duplicate enrollment and waitlist entries.
- Add eligible learners to a FIFO waitlist when an offering is full.
- Automatically promote the earliest eligible waitlisted learner after a drop.
- Recheck prerequisite eligibility during promotion.
- Skip ineligible learners without removing them from the waitlist.
- Protect state-changing workflows against concurrent requests within one application process.
- Expose REST APIs with Swagger documentation.

## Technology Stack

- C#
- .NET 8 Web API
- ASP.NET Core
- xUnit
- Swagger / OpenAPI
- In-memory repository

## Solution Structure

```text
CourseRegistration/
├── src/
│   ├── CourseRegistration.Api/
│   ├── CourseRegistration.Application/
│   ├── CourseRegistration.Domain/
│   └── CourseRegistration.Infrastructure/
├── tests/
│   └── CourseRegistration.Tests/
├── docs/
├── CourseRegistration.sln
└── README.md
```

## Prerequisites

- .NET SDK 8.0 or later compatible with the target framework.
- Visual Studio, Visual Studio Code or another .NET-compatible IDE.

## Build

From the solution root:

```powershell
dotnet restore
dotnet build
```

## Run the API

```powershell
dotnet run --project src/CourseRegistration.Api
```

Open the Swagger URL printed in the terminal. For the configured development environment, it may be:

```text
http://localhost:5033/swagger
```

Use the actual URL displayed by your application if the port differs.

## Run Tests

```powershell
dotnet test --logger "console;verbosity=normal"
```

## Main API Endpoints

| Method | Endpoint                                        | Description                           |
| ------ | ----------------------------------------------- | ------------------------------------- |
| POST   | `/api/enrollments`                              | Enroll or waitlist a learner          |
| DELETE | `/api/enrollments/{offeringId}/{learnerId}`     | Drop enrollment and attempt promotion |
| GET    | `/api/enrollments/course/{offeringId}/status`   | Retrieve offering status              |
| GET    | `/api/enrollments/course/{offeringId}/enrolled` | Retrieve enrolled learners            |
| GET    | `/api/enrollments/course/{offeringId}/waitlist` | Retrieve waitlisted learners          |

## Seed Data

The in-memory repository initializes sample courses, learners, prerequisites and a course offering.

Offering ID:

```text
20000000-0000-0000-0000-000000000001
```

Sample learners:

| Learner | ID                                     |
| ------- | -------------------------------------- |
| Alice   | `30000000-0000-0000-0000-000000000001` |
| Bob     | `30000000-0000-0000-0000-000000000002` |
| Charlie | `30000000-0000-0000-0000-000000000003` |
| David   | `30000000-0000-0000-0000-000000000004` |
| Emma    | `30000000-0000-0000-0000-000000000005` |
| Frank   | `30000000-0000-0000-0000-000000000006` |

Frank is seeded without completing all prerequisites for Distributed Systems, making him useful for testing prerequisite enforcement.

## Documentation

See the `docs/` directory for:

1. Implementation approach.
2. Design specification.
3. Business rules and assumptions.
4. AI usage and prompt log.
5. Testing and verification report.
6. Limitations and trade-offs.
7. Assessment rubric mapping.

## Limitations

- Data is stored in memory and does not survive application restarts.
- Concurrency controls are limited to a single application process.
- Authentication and authorization are not included.
- The application is an assessment implementation, not a production-ready registration platform.

## AI Assistance

AI assistance was used for requirement analysis, architecture discussion, implementation guidance, debugging, test design and documentation. The AI usage log describes the contributions and developer review responsibilities.
