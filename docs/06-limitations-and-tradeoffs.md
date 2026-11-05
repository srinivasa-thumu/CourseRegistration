# Limitations and Trade-offs

## 1. In-Memory Persistence

**Decision:** Use an in-memory repository rather than a relational database.

**Reason:** The assessment focuses on enrollment, capacity, prerequisite validation and waitlist promotion.

**Trade-off:** Application data is lost when the application restarts. The repository cannot provide durable storage or database-level constraints.

**Future improvement:** Replace the in-memory implementation with SQL Server or PostgreSQL and enforce uniqueness and referential integrity through database constraints.

## 2. Process-Local Concurrency

**Decision:** Use SemaphoreSlim in the application service and locks in the in-memory repository.

**Reason:** These mechanisms protect shared state and serialize state-changing workflows within one running application process.

**Trade-off:** They do not coordinate multiple application instances. They are not a distributed locking or transaction strategy.

**Future improvement:** Use database transactions, optimistic concurrency or another suitable distributed coordination strategy when deploying multiple instances.

## 3. Prerequisite Evaluation

**Decision:** Require all configured prerequisites to be completed.

**Reason:** This provides deterministic and straightforward eligibility validation.

**Trade-off:** The current model does not support alternative prerequisite groups, minimum grades, concurrent prerequisite completion or prerequisite exceptions.

**Future improvement:** Introduce a richer prerequisite policy model if the business requires it.

## 4. Ineligible Waitlist Candidates

**Decision:** Skip an ineligible candidate during promotion but retain their original queue position.

**Reason:** An ineligible candidate should not block an eligible learner from using an available seat.

**Trade-off:** A retained learner may be reconsidered during a later promotion. The system does not currently notify learners or automatically remove permanently ineligible candidates.

**Future improvement:** Introduce waitlist expiry, eligibility status tracking and notifications if required.

## 5. API Scope

**Decision:** Expose only the endpoints needed for enrollment, dropping, status, enrolled learners and waitlist retrieval.

**Reason:** The assessment does not require a complete course administration system.

**Trade-off:** Courses, offerings, learners and prerequisite data are not managed through administrative APIs.

**Future improvement:** Add catalog and administration endpoints with appropriate authorization.

## 6. Security

**Decision:** Authentication and authorization are outside the current scope.

**Reason:** The implementation focuses on the registration workflow.

**Trade-off:** The API must not be considered suitable for public production use without identity verification and access control.

**Future improvement:** Add authentication, role-based authorization, input hardening, rate limiting and audit logging.

## 7. Observability and Operations

**Decision:** Keep operational infrastructure minimal.

**Reason:** The assessment is a focused functional implementation.

**Trade-off:** The solution does not include production-grade monitoring, distributed tracing, alerting or audit history.

**Future improvement:** Introduce structured logs, metrics, tracing and operational dashboards.

## 8. Overall Assessment

The selected trade-offs keep the implementation focused and understandable while preserving clear extension points. The current solution demonstrates the core business workflow but should be extended with persistent storage, distributed concurrency control, security and observability before production deployment.
