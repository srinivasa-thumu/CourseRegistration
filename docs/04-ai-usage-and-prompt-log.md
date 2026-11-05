# Assessment Rubric Mapping

> Note: This is an internal checklist mapped to the assessment requirements discussed during development. It is not an official evaluator-issued rubric.

| Assessment area               | Evidence in solution                              | Status                                             |
| ----------------------------- | ------------------------------------------------- | -------------------------------------------------- |
| Course offering capacity      | CourseOffering and enrollment capacity validation | Implemented; tested                                |
| Enrollment workflow           | EnrollmentService                                 | Implemented; tested                                |
| Duplicate prevention          | Existing enrollment and waitlist checks           | Implemented; tested                                |
| Prerequisite validation       | PrerequisiteEligibilityService                    | Implemented; tested                                |
| Multiple prerequisites        | CoursePrerequisite mappings                       | Implemented; tested                                |
| Waitlist management           | WaitlistEntry and queue number                    | Implemented; tested                                |
| FIFO promotion                | Ordered waitlist selection                        | Implemented; tested                                |
| Eligibility recheck           | Eligibility evaluated during promotion            | Implemented; tested                                |
| Ineligible candidate handling | Skip and retain original queue position           | Implemented; tested                                |
| Drop workflow                 | EnrollmentService.DropAsync                       | Implemented; tested                                |
| Invalid and repeated drops    | Validation and failure results                    | Implemented; tested                                |
| Concurrency                   | SemaphoreSlim and repository locking              | Implemented; automated tests reported successful   |
| API endpoints                 | EnrollmentsController                             | Implemented; integration tests reported successful |
| API documentation             | Swagger                                           | Implemented                                        |
| Unit testing                  | xUnit test suite                                  | Implemented; execution reported successful         |
| API integration testing       | WebApplicationFactory                             | Implemented; execution reported successful         |
| Design specification          | docs/02-design-specification.md                   | Drafted                                            |
| Business assumptions          | docs/03-business-rules-and-assumptions.md         | Drafted                                            |
| AI usage disclosure           | docs/04-ai-usage-and-prompt-log.md                | Drafted                                            |
| Testing evidence              | docs/05-testing-and-verification-report.md        | Requires exact counts and manual API results       |
| Limitations and trade-offs    | docs/06-limitations-and-tradeoffs.md              | Drafted                                            |
| Setup instructions            | README.md                                         | Pending                                            |

## Final Submission Checklist

- [ ] Solution builds successfully.
- [ ] All automated tests pass.
- [ ] Actual test counts are recorded.
- [ ] Swagger scenarios are manually verified.
- [ ] FIFO promotion is verified through the API.
- [ ] Prerequisite enforcement is verified through the API.
- [ ] All documentation matches the actual implementation.
- [ ] AI usage is accurately disclosed.
- [ ] README includes setup and execution instructions.
- [ ] No unnecessary packages or temporary files remain.
- [ ] Developer can explain the architecture, business rules and concurrency strategy.
