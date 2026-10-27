ADR 001: Use a Layered .NET Architecture

Status

Accepted for the initial implementation.

Date

2026-10-27

Context

API Navigator is an AI-assisted API discovery and integration application.

The backend will use ASP.NET Core Web API and C#. It will communicate with an AI provider, process API documentation, coordinate application use cases, and support controlled requests to external APIs.

Putting all backend functionality into a single project could make responsibilities harder to separate as the application grows.

We therefore need an initial structure that supports maintainability, testing, and controlled external integrations without introducing unnecessary complexity.

Decision

The backend will initially use a layered solution with three projects:

- "ApiNavigator.Api"
- "ApiNavigator.Application"
- "ApiNavigator.Infrastructure"

Automated tests will be maintained in separate test projects.

ApiNavigator.Api

Responsible for HTTP endpoints, request handling, middleware, and application configuration.

ApiNavigator.Application

Responsible for application use cases, business rules, and interfaces that define the operations required by the application.

ApiNavigator.Infrastructure

Responsible for external integrations and technical implementations, including communication with the AI provider.

The application will use dependency injection to connect interfaces to their implementations.

For example, an interface such as "IAiIntegrationService" will separate application requirements from the specific implementation used to communicate with Meta AI.

Dependency Direction

The API project may depend on the Application project.

The Infrastructure project may depend on the Application project to implement its interfaces.

The Application project must not depend directly on the API or Infrastructure projects.

This keeps application logic independent of web-specific concerns and external integration implementations.

Consequences

Benefits

- Clear separation of responsibilities.
- Easier unit testing of application logic.
- Better isolation of external integrations.
- Greater flexibility when implementation details change.
- A structure suitable for demonstrating backend engineering principles.

Trade-offs

- More projects and configuration than a single-project application.
- Additional interfaces and abstractions to maintain.
- A learning curve for understanding project references and dependency injection.

These costs are acceptable for this portfolio project, provided that we avoid unnecessary abstractions.

Alternatives Considered

Single-project Web API

A single ASP.NET Core project would be simpler to establish and may be sufficient for a small prototype.

However, the project's focus on AI integration, API documentation processing, and external services makes separation of responsibilities useful for learning and maintainability.

Microservices

Multiple independently deployed services could provide stronger deployment and scaling boundaries, but they would introduce operational complexity that the initial application does not require.

Microservices will not be used for the initial implementation.

Review Conditions

This decision may be revisited if the project requirements change significantly or if the layered structure introduces complexity without sufficient benefit.