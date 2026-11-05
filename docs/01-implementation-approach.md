# Course Registration – Implementation Approach

## 1. Objective

Implement a bounded course registration service supporting learner enrollment, course dropping, and FIFO waitlist promotion for a single course offering.

The implementation prioritizes correctness, data consistency, understandable business rules, automated testing, and responsible AI-assisted engineering.

The solution deliberately excludes frontend development, course catalogs, payments, identity integrations, multi-term registration, and external database infrastructure.

## 2. Scope

### Included

- Enroll a learner when capacity is available.
- Add eligible learners to a waitlist when the course is full.
- Prevent duplicate enrollment and waitlist entries.
- Validate learner prerequisites.
- Allow enrolled learners to drop the course.
- Automatically promote the earliest eligible waitlisted learner.
- Handle invalid requests and repeated drops.
- Protect enrollment and promotion operations against concurrent requests.
- Provide REST APIs with Swagger UI.
- Implement automated unit and integration tests.

### Excluded

- User authentication and authorization.
- Course catalog management.
- Payment processing.
- Multiple academic terms.
- Frontend application.
- Persistent external database.
- Distributed deployment infrastructure.

## 3. Technology Stack

| Component         | Technology           |
| ----------------- | -------------------- |
| Language          | C#                   |
| Framework         | ASP.NET Core Web API |
| Runtime           | .NET 8               |
| Architecture      | Layered architecture |
| API documentation | Swagger / OpenAPI    |
| Persistence       | In-memory repository |
| Automated testing | xUnit                |
| Test execution    | .NET CLI             |

## 4. Architecture

The solution follows a lightweight layered architecture.

- **Domain:** Contains core business entities, enums, and domain rules.
- **Application:** Contains enrollment business logic, service interfaces, and repository abstractions.
- **Infrastructure:** Provides the in-memory repository implementation.
- **API:** Exposes REST endpoints and maps requests and responses.

Business rules will remain independent of HTTP concerns and persistence implementation.

## 5. Implementation Strategy

### Phase 1 – Project setup

Create the solution structure, configure project dependencies, and establish the testing environment.

### Phase 2 – Domain modeling

Define course offerings, learners, enrollments, waitlist entries, and enrollment statuses.

### Phase 3 – Enrollment implementation

Validate learner prerequisites, check existing enrollment and waitlist records, and allocate an available seat or create a waitlist entry.

### Phase 4 – Drop and promotion implementation

Allow enrolled learners to drop. When a seat becomes available, examine the waitlist in FIFO order and promote the earliest eligible learner.

### Phase 5 – API implementation

Expose enrollment, drop, course status, enrolled learner, and waitlist endpoints. Provide interactive API documentation through Swagger UI.

### Phase 6 – Automated testing

Test successful operations, duplicate requests, prerequisite failures, waitlist promotion, invalid requests, and concurrency scenarios.

### Phase 7 – Documentation and verification

Complete the design specification, AI usage record, testing report, setup instructions, and limitations report.

## 6. Core Business Decisions

- A learner must satisfy the configured prerequisite before enrollment.
- An eligible learner can join the waitlist when capacity is full.
- Duplicate active enrollment and waitlist records are prohibited.
- Waitlist ordering is maintained using an insertion sequence.
- Eligibility is revalidated at promotion time.
- Ineligible learners are skipped but retained in their original queue position.
- The next eligible learner is promoted.
- Enrollment, dropping, and promotion operations must preserve capacity and uniqueness invariants.

### Prerequisite Data Modeling

Prerequisite relationships and learner course completions will be represented using separate mapping entities.

- `CoursePrerequisite` maps a course to one or more required courses.
- `LearnerCompletedCourse` maps a learner to courses they have successfully completed.

A course may have zero, one, or multiple prerequisites. When multiple prerequisites are configured, all must be satisfied.

If no prerequisite mappings exist for a course, the learner is considered eligible without prerequisite validation.

Prerequisites will only be configured when explicitly required by the course definition or assessment assumptions. Sample prerequisite mappings will be used in automated tests to verify both eligible and ineligible scenarios.

Eligibility will be evaluated during enrollment and revalidated during waitlist promotion.

This approach provides normalized relationships while keeping the implementation limited to the single course-offering workflow.

## 7. Concurrency Strategy

The in-memory implementation will use synchronization around state-changing operations to prevent concurrent requests from exceeding course capacity or promoting multiple learners into the same seat.

The synchronization strategy will be appropriate for a single application process.

The limitations of in-memory persistence and process-local locking will be documented.

## 8. Verification Strategy

Verification will include:

- Successful compilation.
- Automated business logic tests.
- API-level integration tests.
- Duplicate and invalid request tests.
- FIFO promotion tests.
- Concurrent enrollment and drop/promotion tests.
- Manual API verification through Swagger UI.

## 9. AI-Assisted Engineering

AI tools will be used as engineering assistants for requirement analysis, design review, code suggestions, test scenario identification, and documentation.

All AI-generated suggestions will be independently reviewed and verified.

A separate AI usage record will document actual prompts, relevant responses, accepted or rejected suggestions, affected files, and verification evidence.

## 10. Expected Outcome

A runnable, documented, and tested ASP.NET Core service demonstrating reliable enrollment, dropping, and FIFO waitlist promotion for one course offering.

The implementation will prioritize correctness and engineering judgment over additional features or infrastructure.

## 11. Infrastructure and API Implementation

The application uses an in-memory repository implementing the application-layer repository abstraction.

The repository maintains course, offering, learner, prerequisite, completed-course, enrollment, and waitlist collections.

Sample data is seeded during repository initialization to support repeatable API demonstrations and manual verification.

The ASP.NET Core Web API exposes enrollment, drop, enrollment status, enrolled learner, and waitlist endpoints.

Swagger UI provides an interactive interface for reviewers to execute API requests without requiring a separate frontend.

The EnrollmentService and repository are registered as singleton dependencies to maintain shared in-memory state and process-level synchronization.

Repository collection access is protected by synchronization, while complete state-changing workflows are serialized by the application service.
