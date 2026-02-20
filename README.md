# Backend Developer Code Challenge

## Introduction
Welcome to the Backend Developer Technical Assessment! This test is designed to evaluate your proficiency in building REST APIs using .NET 8, focusing on clean architecture, business logic, and testing practices. We have prepared a set of tasks and questions that cover a spectrum of skills, ranging from fundamental concepts to more advanced topics.

**Note:** This assessment focuses on API development, architecture, and testing. During the interview, we'll discuss your experience with databases, event-driven design, Docker/Kubernetes, and cloud platforms.

## Tasks
Complete the provided tasks to demonstrate your ability to work with .NET 8, ASP.NET Core Web API, and unit testing. Adjust the complexity based on your experience level.

## Questions
Answer the questions to showcase your understanding of the underlying concepts and best practices associated with the technologies in use.

## Time Limit
This assessment is designed to take approximately 1-2 hours to complete. Please manage your time effectively.

## Setup the Repository
Make sure you have .NET 8 SDK installed
- Install dependencies with `dotnet restore`
- Build the project with `dotnet build`
- Run the project with `dotnet run --project CodeChallenge.Api`
- Navigate to `https://localhost:5095/swagger` to see the API documentation

## Prerequisite
Start the test by forking this repository, and complete the following tasks:

---

## Task 1
**Assignment:** Implement a REST API with CRUD operations for messages. Use the provided `IMessageRepository` and models to create a `MessagesController` with these endpoints:
- `GET /api/v1/organizations/{organizationId}/messages` - Get all messages for an organization
- `GET /api/v1/organizations/{organizationId}/messages/{id}` - Get a specific message
- `POST /api/v1/organizations/{organizationId}/messages` - Create a new message
- `PUT /api/v1/organizations/{organizationId}/messages/{id}` - Update a message
- `DELETE /api/v1/organizations/{organizationId}/messages/{id}` - Delete a message

**Question 1:** Describe your implementation approach and the key decisions you made.

**Answer 1:** I implemented the controller by calling `IMessageRepository` directly since it was already injected and registered in DI. Each endpoint follows standard REST conventions:
- **GET** returns `200 OK` with the data (or `404` if not found)
- **POST** validates input, checks for duplicate titles, creates the message, and returns `201 Created`
- **PUT** validates input, checks the message exists, prevents duplicate titles , updates fields, and returns `204 No Content`
- **DELETE** attempts removal and returns `204` on success or `404` if not found

For validation, I check that `Title` and `Content` are not empty and use `GetByTitleAsync` to keep unique titles per organization and return `409 Conflict` on duplicates.

**Question 2:** What would you improve or change if you had more time?

**Answer 2:**
- **Add a business logic layer** — Implement `IMessageLogic` so the controller stays thin and all rules stay in one place
- Add `skip`/`take` parameters to `GetAll` so it doesn't return everything at once
- **Structured errors** — Return `ProblemDetails` instead of plain strings.
- **Unit tests** — Test the controller with a mocked repository covering all success and failure scenarios
- **Logging** — Use the injected `ILogger` to log all actions and error details.

---

## Task 2
**Assignment:** Separate business logic from the controller and add proper validation.
1. Implement `MessageLogic` class (implement `IMessageLogic`)
2. Implement Business Rules:
   - Title must be unique per organization
   - Content must be between 10 and 1000 characters
   - Title is required and must be between 3 and 200 characters
   - Can only update or delete messages that are active (`IsActive = true`)
   - UpdatedAt should be set automatically on updates
3. Return appropriate result types (see `Logic/Results.cs`)
4. Update Controller to use `IMessageLogic` instead of directly using the repository

**Question 3:** How did you approach the validation requirements and why?

**Answer 3:** I created a private `ValidateMessage` helper method inside `MessageLogic` that checks both Title and Content rules in one place. Title must be 3–200 characters and Content must be 10–1000 characters. If any rule fails, the method collects all errors into a dictionary and returns a `ValidationError` result. Business rules like "only active messages can be updated or deleted" are checked separately after fetching the message. I kept all validation inside the logic layer so the controller stays simple and only maps results to HTTP responses.

**Question 4:** What changes would you make to this implementation for a production environment?

**Answer 4:**
- **Use a real database** — Replace the in-memory repository with Entity Framework Core or another ORM backed by a real database like SQL Server
- **Use FluentValidation** — Move validation rules into dedicated validator classes for better reusability and testability
- **Add logging** — Log all operations with structured logging for monitoring and debugging, by using Serilog, appinsights/new relic tools
- **Add authentication and authorization** — Ensure users can only access messages for organizations they belong to
- **Add pagination** — Return paged results from `GetAll` to handle large datasets


---

## Task 3
**Assignment:** Write comprehensive unit tests for your business logic.
1. Create `CodeChallenge.Tests` project (xUnit)
2. Add required packages: xUnit, Moq, FluentAssertions
3. Write Tests for MessageLogic covering these scenarios:
   - Test successful creation of a message
   - Test duplicate title returns Conflict
   - Test invalid content length returns ValidationError
   - Test update of non-existent message returns NotFound
   - Test update of inactive message returns ValidationError
   - Test delete of non-existent message returns NotFound

**Question 5:** Explain your testing strategy and the tools you chose.

**Question 6:** What other scenarios would you test in a real-world application?

commit the code as task-3
