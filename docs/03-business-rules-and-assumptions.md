# Business Rules and Assumptions

## 1. Enrollment Rules

1. A learner can have only one active enrollment in a particular course offering.
2. A learner who is already enrolled cannot join the same offering's waitlist.
3. A learner who is already waitlisted cannot submit another waitlist request for the same offering.
4. Enrollment is allowed only when the learner satisfies the course prerequisites.
5. If the offering has available capacity, the learner is enrolled immediately.
6. If the offering is full, an eligible learner is added to the waitlist.
7. A learner's enrollment in one offering does not automatically prevent enrollment in another offering.

## 2. Prerequisite Rules

1. Prerequisites are configured at the course level.
2. An offering inherits the prerequisites of its associated course.
3. A learner must have completed all configured prerequisite courses.
4. If a course has no configured prerequisites, the learner is eligible.
5. Prerequisite eligibility is evaluated when the learner initially requests enrollment.
6. Prerequisite eligibility is evaluated again when a waitlisted learner is considered for promotion.
7. If a waitlisted learner is no longer eligible, they are skipped for the current promotion and retained in the queue.

## 3. Waitlist Rules

1. Waitlist entries are ordered by an increasing queue number for each offering.
2. The lowest queue number represents the earliest request.
3. Promotion follows FIFO ordering among eligible learners.
4. Ineligible learners do not block eligible learners behind them.
5. Skipped learners retain their original queue numbers.
6. A promoted learner's waitlist entry is removed when their enrollment is created.
7. If no eligible waitlisted learner exists, the seat remains available.

## 4. Drop Rules

1. Only an actively enrolled learner can drop an offering.
2. A learner cannot drop the same enrollment more than once.
3. Dropping an enrollment triggers an attempt to promote an eligible waitlisted learner.
4. The drop and promotion state changes are performed together within the repository's locked operation.
5. If promotion is not possible, the dropped seat remains available.

## 5. Capacity Rules

1. Capacity must be a positive integer.
2. The number of active enrollments must not exceed offering capacity.
3. A full offering accepts eligible learners onto its waitlist rather than exceeding capacity.
4. A promoted learner occupies the seat released by the dropped learner.

## 6. Assumptions

- The assessment focuses on one course registration workflow and does not require a complete course catalog.
- Course offerings, learners, prerequisite mappings and completed-course records are seeded in memory.
- A course may have multiple prerequisites.
- A learner's completed courses are treated as the source of truth for prerequisite eligibility.
- A learner may remain on the waitlist even if their eligibility changes after joining it.
- The first eligible learner in FIFO order is promoted.
- Queue numbers are maintained independently for each offering.
- The application is assumed to run as a single process.
- Application state does not need to survive a restart for this assessment.
- Authentication, authorization, payments, notifications and administrative catalog management are outside the current scope.

## 7. Decisions and Rationale

| Decision                                 | Rationale                                                                                                        |
| ---------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| Separate Course and CourseOffering       | Course information and offering-specific capacity have different responsibilities.                               |
| Normalized prerequisite mapping          | Supports multiple prerequisites without embedding a collection in the Course entity.                             |
| Normalized learner completion mapping    | Supports flexible prerequisite evaluation.                                                                       |
| FIFO queue number                        | Makes waitlist ordering explicit and deterministic.                                                              |
| Skip but retain ineligible learners      | Prevents an ineligible learner from blocking eligible candidates while preserving their original queue position. |
| Recheck eligibility during promotion     | Eligibility may change after the learner joins the waitlist.                                                     |
| In-memory repository                     | Keeps the assessment focused on the business workflow.                                                           |
| SemaphoreSlim in the application service | Serializes state-changing workflows within a single process.                                                     |
| Repository lock for atomic state changes | Protects shared in-memory collections during related mutations.                                                  |

## 8. Out-of-Scope Decisions

The following capabilities are intentionally excluded:

- Multiple course registration periods.
- Course cancellation and timetable conflict management.
- Waitlist expiry.
- Manual waitlist priority overrides.
- Notifications to learners.
- Database persistence and distributed locking.
- Authentication and authorization.
