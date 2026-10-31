# API Navigator — System Architecture

## 1. Purpose

API Navigator is an API discovery and integration assistant designed to help developers find APIs, understand API documentation, generate integration examples, and test approved API requests.

The planned stack is a React and TypeScript frontend and an ASP.NET Core Web API backend. AI will provide optional assistance, initially using Meta AI where appropriate. Core discovery and documentation features must remain useful when an AI provider is unavailable.

This document describes the proposed architecture. It does not imply that the application components have already been implemented.

## 2. Architectural Goals

The architecture aims to provide:

- Clear separation of responsibilities.
- Maintainable and testable application code.
- Discovery of APIs from a local catalogue and supported public sources.
- Documentation-grounded AI assistance and code generation.
- AI-provider independence.
- Controlled and secure interaction with external systems.
- Automated build and test workflows.
- A path towards future cloud deployment.

## 3. High-Level Architecture

The application consists of these logical components:

1. React and TypeScript frontend.
2. ASP.NET Core Web API.
3. API catalogue for known APIs.
4. API discovery service for searching supported external directories.
5. OpenAPI specification processor.
6. AI assistance service accessed through an application-owned interface.
7. Request validation and controlled execution services.
8. Adapters for external directories, AI providers, and supported APIs.

The frontend communicates with the backend over HTTPS using JSON. The backend coordinates application operations. External directory access, AI-provider calls, and calls to APIs selected for testing take place through backend-controlled components.

## 4. Component Responsibilities

### 4.1 Frontend

**Technology:** React and TypeScript.

Responsibilities:

- Let users search known APIs and supported external sources.
- Display API details and documentation links.
- Submit natural-language discovery and integration questions.
- Display API comparisons and generated integration examples.
- Present request validation results.
- Display API responses and errors.

The frontend must not contain private AI-provider credentials or other server-side secrets.

### 4.2 Backend API

**Technology:** ASP.NET Core Web API using C#.

Responsibilities:

- Expose application endpoints.
- Validate incoming requests.
- Coordinate application use cases.
- Handle errors consistently.
- Apply security controls and request limits.
- Coordinate API discovery, documentation processing, and AI assistance through application services.
- Coordinate only approved and controlled external API requests.

HTTP concerns must remain separate from application logic and infrastructure integrations.

### 4.3 API Catalogue

The API catalogue provides searchable records for known APIs. The initial prototype may use a small, curated dataset rather than requiring a database.

An API record may include:

- API name and description.
- Base URL, where appropriate.
- Documentation URL.
- OpenAPI specification location, where available.
- Authentication type, if known.
- Supported capabilities and source attribution.

External discovery results do not have to be permanently saved in the catalogue. Persistence can be added later if the requirements justify it.

### 4.4 API Discovery Service

The API discovery service searches sources outside API Navigator's own catalogue. A provider-independent application contract has been introduced before connecting a real directory.

The current contract consists of:

- `ApiDiscoveryQuery` — a required search term and optional category. Input is trimmed, and a blank search term is rejected.
- `ApiDiscoveryResult` — a normalized API listing that can retain its name, description, base URL, documentation URL, OpenAPI specification URL, tags, and source attribution.
- `IApiDiscoverySource` — an asynchronous source adapter contract that accepts a query and cancellation token and returns normalized results.

Implementations belong in Infrastructure. The Application layer does not depend on a particular directory, its response format, or its SDK. No external directory has been connected yet.

Responsibilities:

- Accept structured search criteria, such as keywords or categories.
- Query supported public API directories or registries through source-specific adapters.
- Normalize results into a common API summary format.
- Preserve the source and original documentation links.
- Handle unavailable sources, timeouts, rate limits, and partial results.
- Avoid presenting unverified metadata as confirmed facts.

The first adapter connects to APIs.guru through its public `/v2/list.json` endpoint. It selects each API's preferred version, filters cached metadata by search term and category, and preserves specification/documentation links and source attribution. The integration does not download or execute the listed API specifications. APIs.guru is one source, not a complete inventory of every public API. Its catalogue is community-maintained, so individual listings still require verification.

### 4.5 OpenAPI Specification Processor

The OpenAPI processor retrieves and interprets supported API specifications.

Responsibilities may include:

- Reading supported OpenAPI documents.
- Identifying endpoints and HTTP methods.
- Extracting parameters, authentication schemes, and request/response formats.
- Providing relevant specification details to other application services.

Remote specifications must be validated and constrained by size and timeout limits. Documentation is untrusted input: the application must not execute instructions embedded in it or treat those instructions as trusted system instructions.

### 4.6 AI Assistance Service

AI is an optional capability for tasks that benefit from language understanding or generation, such as:

- Interpreting a developer's natural-language requirements.
- Explaining API documentation.
- Comparing discovered APIs against stated requirements.
- Generating integration examples grounded in retrieved documentation.

The Application layer will define an interface for the AI capability, for example `IAIService`. Infrastructure will provide the provider-specific implementation. Meta AI may be the initial provider, but provider-specific request formats and credentials must remain inside the relevant adapter.

The application must support a clear failure path when AI is unavailable. Core catalogue search, external directory search where available, and documentation display must not require an AI call. A deterministic mock implementation may be used in tests.

AI responses and generated code are untrusted output. They must be validated and reviewed; the application must not automatically execute generated code.

### 4.7 Request Validation and Execution

This component will help users prepare and test approved API requests.

Responsibilities may include:

- Validate destination URLs and supported HTTP methods.
- Validate parameters and headers.
- Apply request timeouts and response-size limits.
- Display response status codes and results.
- Restrict access to unsafe destinations and internal network addresses.
- Require clear user intent before executing requests that may change remote data.

The application must mitigate server-side request forgery (SSRF), unsafe redirects, credential leakage, and unintended data modification. It must not automatically execute arbitrary code or requests generated by AI.

### 4.8 External Systems

External systems include API directories, AI providers, API documentation hosts, and the APIs a user chooses to test.

Each integration must have a defined boundary, timeout, error-handling strategy, and security policy. External sources may be unavailable, rate-limited, incomplete, or subject to terms of use.

## 5. Proposed Backend Structure

The backend is planned to use the following solution structure:

```text
src/
  ApiNavigator.Api/
  ApiNavigator.Application/
  ApiNavigator.Infrastructure/

tests/
  ApiNavigator.UnitTests/
  ApiNavigator.IntegrationTests/

frontend/
docs/
  architecture.md
  decisions/
```

**ApiNavigator.Api** contains HTTP endpoints, middleware, dependency-injection configuration, and API-specific configuration.

**ApiNavigator.Application** contains use cases, business rules, and interfaces for capabilities such as API discovery and AI assistance. It must not depend directly on Infrastructure or web-specific API concerns.

**ApiNavigator.Infrastructure** contains implementations for external integrations, including public API directory adapters, AI-provider adapters, and external HTTP access.

**Tests** contain automated tests for application behaviour and integration boundaries.

The final structure will be adjusted when the solution is created. Interfaces should be introduced where they define meaningful boundaries, not simply to add unnecessary abstraction.

## 6. Dependency Direction

- `ApiNavigator.Api` may depend on `ApiNavigator.Application`.
- `ApiNavigator.Infrastructure` may depend on `ApiNavigator.Application` to implement its interfaces.
- `ApiNavigator.Application` must not depend directly on `ApiNavigator.Api` or `ApiNavigator.Infrastructure`.
- Provider-specific AI code and external-source-specific discovery code belong in Infrastructure.
- Application use cases should depend on application-owned interfaces rather than provider SDKs or external-source response formats.

## 7. Security Principles

The system will follow these principles:

- Keep secrets out of source control and frontend code.
- Store private AI credentials on the backend using appropriate secret configuration.
- Use HTTPS for deployed application traffic.
- Validate and constrain remote URLs and redirects.
- Apply timeouts, response-size limits, and rate limits where appropriate.
- Avoid logging secrets and unnecessary sensitive data.
- Treat remote documentation and AI output as untrusted input.
- Require explicit controls for external API execution.
- Restrict access to internal network destinations to mitigate SSRF.
- Respect the terms, rate limits, and attribution requirements of external sources.
- Use managed secret storage when deployed to a suitable cloud environment.

## 8. Development and Deployment

Development will initially take place locally. GitHub will be used for source control, and GitHub Actions will automate builds and tests after the .NET solution exists.

Cloud deployment will be considered after the initial application works locally. Microsoft Azure is the planned cloud platform to evaluate. An API gateway may be introduced later if the application needs centralised traffic management, rate limiting, or API governance.

## 9. Implementation Sequence

The planned implementation sequence is:

1. Create the .NET solution and establish the layered project references.
2. Implement a small, searchable curated API catalogue.
3. Define the application contract for API discovery and connect the first public directory adapter (APIs.guru).
4. Expose normalized external discovery results with source links through the backend API.
5. Retrieve and process supported OpenAPI specifications safely.
6. Add an AI service interface and a deterministic mock for tests.
7. Integrate Meta AI as the initial provider, keeping it replaceable and optional.
8. Generate documentation-grounded explanations and integration examples.
9. Add controlled request validation and execution.
10. Build the React and TypeScript frontend around the working backend use cases.
11. Add automated unit and integration tests.
12. Prepare cloud deployment and document the deployed architecture.

The sequence may be adjusted based on what we learn, but each stage should be tested before the next is introduced.

## 10. Future Considerations

Potential future improvements include persistent storage for saved API projects, user accounts and authorisation, additional discovery sources, additional AI providers, more programming languages for code generation, cloud deployment, API gateway integration, and improved monitoring.

These additions will be driven by demonstrated requirements rather than introduced prematurely.