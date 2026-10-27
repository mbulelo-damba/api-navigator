# API Navigator

**API Navigator is an AI-assisted API discovery and integration platform that helps developers find public APIs, understand their documentation, evaluate their suitability, and build integrations more efficiently.**

The project is being developed as a practical portfolio application using ASP.NET Core Web API, C#, React, and TypeScript. AI assistance is designed to be optional: core API discovery should remain useful even when an AI provider is unavailable.

## Overview

Integrating an external API can involve finding a suitable service, understanding its documentation, checking authentication requirements, and writing and testing integration code.

API Navigator aims to bring these steps into one user-friendly workflow. It will start with a small, working prototype and expand incrementally.

## Goals

- Help developers discover APIs relevant to a development task.
- Make API documentation and OpenAPI specifications easier to understand.
- Generate integration examples grounded in the selected API's documentation.
- Help developers validate requests before sending them.
- Support controlled testing of approved API requests.
- Demonstrate maintainable backend architecture and optional AI integration.

## Planned Features

### 1. API Discovery

Search an initial curated catalogue and progressively add adapters for external API directories. Discovery results should include useful metadata and links to their original sources.

### 2. Documentation Assistant

Process supported API documentation and OpenAPI specifications, then explain endpoints, parameters, responses, and authentication requirements in plain language.

### 3. Documentation-Grounded Code Examples

Generate example integration code based on verified documentation for the selected API. Generated code will be presented for developer review rather than treated as automatically trusted.

### 4. Request Validation

Help developers check HTTP methods, endpoint URLs, parameters, headers, and request formats before sending requests.

### 5. Controlled API Testing

Allow developers to send approved requests to supported external APIs and inspect responses. Requests that could modify remote data should require clear user intent and appropriate safeguards.

### 6. Optional AI Assistance

Use an AI provider to explain documentation and help with integration tasks. Provider-specific code will sit behind an application-owned interface so the provider can be replaced. Meta AI is the planned initial integration, subject to confirming the appropriate API access and integration method.

## Planned Technology Stack

| Component | Technology |
|---|---|
| Backend API | ASP.NET Core Web API |
| Backend language | C# |
| Backend architecture | Layered solution: API, Application, Infrastructure |
| Frontend | React and TypeScript |
| API description format | OpenAPI |
| AI integration | Provider adapter behind an application interface; Meta AI planned initially |
| Database | To be decided if persistence is needed for the prototype |
| Version control | Git and GitHub |
| Continuous integration | GitHub Actions |
| Future cloud hosting | Microsoft Azure |

Technology choices may evolve as the prototype is implemented.

## Architecture

The backend is planned around three projects:

- **ApiNavigator.Api** — HTTP endpoints, middleware, and application configuration.
- **ApiNavigator.Application** — application use cases, business rules, and interfaces.
- **ApiNavigator.Infrastructure** — technical implementations and external integrations, including API directory and AI provider adapters.

The intended dependency direction is:

- API depends on Application.
- Infrastructure implements interfaces defined by Application.
- Application does not directly depend on API or Infrastructure.

The initial architecture is documented in [System Architecture](docs/architecture.md) and [ADR 001: Layered .NET Architecture](docs/decisions/001-layered-architecture.md).

## Security Principles

External API documentation and AI-generated content are untrusted inputs. The implementation will account for risks such as:

- Keeping credentials and AI provider secrets on the backend, never in frontend code or source control.
- Validating user input and external URLs.
- Mitigating server-side request forgery (SSRF), including attempts to reach internal or otherwise restricted network addresses.
- Applying timeouts and response-size limits when retrieving external documentation or calling services.
- Requiring clear user intent for requests that can change remote data.
- Avoiding unnecessary sensitive data in prompts and logs.
- Treating AI-generated explanations and code as suggestions that require review.

## Development Roadmap

- [x] Create the GitHub repository.
- [x] Document the initial architecture and architectural decision.
- [x] Create the .NET solution and initial project structure.
- [ ] Implement the initial curated API catalogue.
- [ ] Define API discovery contracts and add the first external directory adapter.
- [ ] Retrieve and process supported OpenAPI specifications safely.
- [ ] Add an AI interface and a test substitute.
- [ ] Integrate the initial AI provider once access and integration details are confirmed.
- [ ] Generate documentation-grounded integration examples.
- [ ] Implement controlled API request testing.
- [ ] Build the React and TypeScript frontend.
- [ ] Add automated unit and integration tests.
- [ ] Configure GitHub Actions to restore, build, and test the solution.
- [ ] Prepare and document cloud deployment.

## Current Status

**Planning and architecture documentation are in place. The initial .NET solution, layered projects, test projects, and a basic health endpoint are scaffolded. API discovery and the frontend have not yet been implemented.**

The next milestone is to implement the first API discovery use case and add meaningful automated tests. The prototype will then be built incrementally, with documentation updated as functionality is introduced.

## Future Direction

After the core prototype works, potential improvements include more API directory sources, additional AI providers, saved integration projects, more languages for code examples, cloud deployment, API gateway integration, and improved observability.

## Vision

Make API integration easier to discover, understand, implement, and test—so developers can spend less time navigating documentation and more time building useful software.
