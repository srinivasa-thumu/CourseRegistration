# Testing and Verification Report

## 1. Objective

The purpose of testing is to verify the course registration business rules, application workflows, API responses and protection against concurrent state changes.

## 2. Test Environment

- Runtime: .NET 8
- Test framework: xUnit
- API test framework: ASP.NET Core integration testing with WebApplicationFactory
- Persistence: In-memory repository
- Execution command:

```powershell
dotnet test --logger "console;verbosity=normal"
```

## 3. Unit Test Coverage

The unit tests cover the following scenarios:

| Scenario                       | Expected behaviour                                 |
| ------------------------------ | -------------------------------------------------- |
| Enroll with available capacity | Learner is enrolled                                |
| Enroll when offering is full   | Learner is waitlisted                              |
| Duplicate enrollment           | Duplicate request is rejected                      |
| Duplicate waitlist request     | Duplicate request is rejected                      |
| Missing prerequisite           | Enrollment is rejected                             |
| FIFO promotion                 | Earliest eligible learner is promoted              |
| Ineligible waitlisted learner  | Candidate is skipped and retained                  |
| Drop non-enrolled learner      | Clear failure result                               |
| Repeated drop                  | No duplicate state mutation                        |
| Nonexistent offering           | Clear failure result                               |
| Nonexistent learner            | Clear failure result                               |
| Empty identifiers              | Invalid request is rejected                        |
| Concurrent enrollment          | Capacity is not exceeded                           |
| Concurrent drops               | Promotions preserve capacity and state consistency |

## 4. API Integration Test Coverage

The API integration tests cover:

- Successful enrollment response.
- Waitlist response when capacity is full.
- Duplicate enrollment response.
- Prerequisite failure response.
- Successful drop response.
- Drop request for a learner who is not enrolled.
- Course offering status response.
- Swagger endpoint availability.

## 5. Execution Results

Record the actual results from the local test execution.

| Metric      | Result     |
| ----------- | ---------- |
| Build       | Successful |
| Total tests | 25         |
| Passed      | 25         |
| Failed      | 0          |
| Skipped     | 0          |
| Warnings    | 0          |

**Execution status:** The developer reported that the build and automated test run completed successfully. Exact counts should be copied from the test runner output before submission.

## 6. Manual API Verification

Use Swagger UI to verify the following workflow:

1. Enroll three eligible learners into the offering.
2. Submit two additional eligible learners and confirm they are waitlisted.
3. Retrieve offering status and verify capacity and waitlist counts.
4. Drop the first enrolled learner.
5. Verify that the earliest eligible waitlisted learner is promoted.
6. Verify that the remaining waitlist order is correct.
7. Attempt enrollment with a learner who has not completed all prerequisites.
8. Attempt a repeated drop and verify that the application returns a clear failure response.

Record the actual manual verification results:

| Scenario                   | Result |
| -------------------------- | ------ |
| Enrollment and waitlisting | 10     |
| Offering status            | 5      |
| FIFO promotion             | 3      |
| Prerequisite enforcement   | 3      |
| Repeated drop              | 3      |

## 7. Verification Conclusion

The solution has successfully completed the reported automated build and test execution. Manual API verification should also be completed and recorded before final submission.

Passing automated tests demonstrates that the covered scenarios work as expected; it does not establish production readiness or guarantee that every possible edge case has been tested.
