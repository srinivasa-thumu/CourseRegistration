# Course Registration System — Design Specification

## 1. Overview

The Course Registration System manages learner enrollment in a specific course offering. It supports capacity validation, prerequisite checks, duplicate prevention, waitlisting, and automatic promotion from the waitlist when an enrolled learner drops the course.

The implementation uses a layered architecture to separate business rules, application workflows, infrastructure concerns, and HTTP API responsibilities.

## 2. Technology Stack

- **Language:** C#
- **Framework:** .NET 8 Web API
- **API documentation and testing:** Swagger / OpenAPI
- **Testing:** xUnit
- **Persistence:** In-memory repository
- **Dependency injection:** Built-in .NET dependency injection

## 3. Architecture

The solution is divided into four primary projects.

| Layer          | Responsibility                                                         |
| -------------- | ---------------------------------------------------------------------- |
| Domain         | Core entities and domain invariants                                    |
| Application    | Enrollment workflows, prerequisite evaluation and repository contracts |
| Infrastructure | In-memory repository implementation and seeded data                    |
| API            | HTTP endpoints, request validation and response mapping                |
| Tests          | Unit and API integration tests                                         |

### Dependency direction

The API depends on Application and Infrastructure for composition and dependency registration.

Application depends on Domain and defines interfaces for infrastructure services.

Infrastructure implements the Application interfaces and uses Domain entities.

Domain does not depend on any other project.

This keeps business rules independent of the storage mechanism and HTTP framework.

## 4. Domain Model

### Course

Represents a course in the catalog.

Key attributes:

- CourseId
- Name

### CourseOffering

Represents a particular offering of a course.

Key attributes:

- CourseOfferingId
- CourseId
- Capacity

Capacity belongs to the offering rather than the course because different offerings of the same course may have different seat limits.

### Learner

Represents a learner who can register for an offering.

Key attributes:

- LearnerId
- Name

### CoursePrerequisite

Maps a course to a prerequisite course.

A course can have multiple prerequisite records. A learner must have completed every configured prerequisite to be eligible.

### LearnerCompletedCourse

Maps a learner to a course they have completed.

This mapping supports prerequisite evaluation without storing a collection of course identifiers directly inside the Learner entity.

### Enrollment

Represents an active enrollment for a learner in a course offering.

The application prevents a learner from having duplicate active enrollments in the same offering.

### WaitlistEntry

Represents a learner waiting for a seat in an offering.

Key attributes:

- WaitlistEntryId
- CourseOfferingId
- LearnerId
- QueueNumber
- CreatedAt

QueueNumber determines FIFO ordering.

## 5. Business Rules

1. A learner can enroll in an offering only once.
2. A learner can join an offering's waitlist only once.
3. A learner must satisfy all configured prerequisites before enrollment.
4. If no prerequisites are configured, the learner is considered eligible.
5. If capacity is available, an eligible learner is enrolled.
6. If capacity is full, an eligible learner is added to the waitlist.
7. When an enrolled learner drops, the earliest eligible waitlisted learner is promoted.
8. Eligibility is checked again during promotion.
9. An ineligible waitlisted learner is skipped but retained in their original queue position.
10. Invalid requests and repeated drops must not corrupt enrollment or waitlist state.

## 6. Enrollment Workflow

1. Validate the offering and learner identifiers.
2. Verify that the offering exists.
3. Verify that the learner exists.
4. Check whether the learner is already enrolled or waitlisted.
5. Evaluate prerequisite eligibility.
6. Compare the current enrollment count with the offering capacity.
7. If a seat is available, create an Enrollment.
8. Otherwise, create a WaitlistEntry with the next queue number.
9. Return an appropriate application result to the API.

## 7. Drop and Promotion Workflow

1. Validate the offering and learner identifiers.
2. Verify that the offering exists.
3. Find the learner's active enrollment.
4. If no active enrollment exists, return a clear failure result.
5. Examine waitlisted learners in FIFO order.
6. Re-evaluate each candidate's prerequisite eligibility.
7. Select the first eligible candidate.
8. Create the promoted learner's Enrollment.
9. Remove the dropped learner's Enrollment and the selected learner's WaitlistEntry as one repository operation.
10. If no eligible candidate exists, remove only the dropped enrollment.

An ineligible candidate remains on the waitlist. This preserves the original queue position and allows eligibility to be evaluated again during a later promotion.

## 8. Concurrency Strategy

The application service uses a `SemaphoreSlim` to serialize state-changing enrollment and drop workflows within the running application process.

The in-memory repository also uses a lock around shared collection operations.

The drop-and-promotion repository operation validates the relevant records and performs the related state changes under the same repository lock. This reduces the risk of an intermediate state in which a seat has been released but the promoted learner has not been enrolled.

This strategy provides process-local concurrency protection. It is not a distributed locking mechanism and does not coordinate multiple application instances.

## 9. API Design

The API exposes the following operations:

| Method | Endpoint                                        | Purpose                                         |
| ------ | ----------------------------------------------- | ----------------------------------------------- |
| POST   | `/api/enrollments`                              | Enroll a learner or add them to the waitlist    |
| DELETE | `/api/enrollments/{offeringId}/{learnerId}`     | Drop an active enrollment and attempt promotion |
| GET    | `/api/enrollments/course/{offeringId}/status`   | Retrieve capacity and enrollment status         |
| GET    | `/api/enrollments/course/{offeringId}/enrolled` | Retrieve enrolled learners                      |
| GET    | `/api/enrollments/course/{offeringId}/waitlist` | Retrieve waitlisted learners                    |

The API translates application results into appropriate HTTP responses, including success, accepted, validation failure, not found, conflict, and prerequisite failure responses.

## 10. Persistence Strategy

The implementation uses an in-memory repository with seeded data.

This keeps the assessment focused on business logic, API design, and testing rather than database setup.

The repository interface allows a persistent implementation to replace the in-memory implementation later without changing the core application workflow.

The current implementation does not provide persistence across application restarts.

## 11. Testing Strategy

Testing is divided into two levels:

### Unit tests

Validate business workflows such as:

- Successful enrollment
- Capacity exhaustion and waitlisting
- Duplicate prevention
- Prerequisite validation
- FIFO promotion
- Skipping ineligible waitlisted learners
- Invalid and repeated drops
- Concurrent state-changing operations

### API integration tests

Validate HTTP behaviour through the application host, including:

- Enrollment responses
- Waitlist responses
- Duplicate request responses
- Prerequisite failure responses
- Drop responses
- Offering status
- Swagger availability

Actual test execution results are documented separately in the testing report.

## 12. Known Limitations

- Data is held in memory and is lost when the application restarts.
- Concurrency protection applies only within a single application process.
- Authentication and authorization are outside the assessment scope.
- No production database or distributed transaction mechanism is included.
- Course and learner data are seeded rather than managed through catalog APIs.
- The implementation does not include a frontend.

## 13. Future Improvements

If the system were extended for production use, possible improvements would include:

- Persistent storage using SQL Server or PostgreSQL.
- Database transactions and concurrency control.
- Unique constraints for enrollment and waitlist records.
- Distributed coordination for multiple application instances.
- Authentication and role-based authorization.
- Structured logging, monitoring and audit history.
- Administrative APIs for courses, offerings, learners and prerequisites.
