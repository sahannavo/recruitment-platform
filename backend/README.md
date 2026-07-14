# Kaveesha's Module — AI Service, Notification Service, Unit & Integration Tests

Part of the AI-Powered Recruitment and Talent Management Platform backend (ASP.NET Core 8).

## What's in here

```
Kaveesha-Backend/
├── Services/
│   ├── AI/
│   │   ├── IAIService.cs            interface: ParseResume, ExtractSkills, MatchCandidateToJob, RankCandidates, GenerateFeedback
│   │   ├── AIService.cs             implementation — calls an OpenAI-compatible endpoint, falls back to keyword matching
│   │   └── AIServiceOptions.cs      config binding (Provider, ApiKey, Endpoint, Model)
│   └── Notification/
│       ├── INotificationService.cs  interface: SendEmail, SendSms, SendInterviewReminder, SendStatusUpdate
│       ├── NotificationService.cs   implementation — orchestrates email/SMS + templates
│       ├── IEmailSender.cs / SendGridEmailSender.cs
│       ├── ISmsSender.cs / TwilioSmsSender.cs
│       ├── NotificationTemplates.cs
│       └── NotificationServiceOptions.cs
├── DTOs/
│   ├── AI/AIDtos.cs                 ResumeParseResult, MatchScoreDto, CandidateRankingDto, AIFeedbackDto
│   └── Notification/NotificationDto.cs  NotificationDto (with TypeName for entity compat), NotificationResultDto
├── Helpers/
│   └── AIScoreCalculator.cs         CalculateMatchScore, RankCandidates — delegates to IAIService
├── Extensions/
│   └── KaveeshaServiceExtensions.cs `AddKaveeshaModule(configuration)` — DI registration
├── Contracts/
│   └── TeamContracts.cs             placeholder interfaces mirroring teammates' modules (see note below)
├── Tests/
│   ├── Unit/                        AuthServiceTests, CandidateServiceTests, JobServiceTests,
│   │                                 ApplicationServiceTests, InterviewServiceTests, FeedbackServiceTests,
│   │                                 AIServiceTests, NotificationServiceTests
│   └── Integration/                 ApiIntegrationTests, DatabaseIntegrationTests
├── RecruitmentAPI.Kaveesha.csproj
├── Tests/RecruitmentAPI.Tests.csproj
└── appsettings.snippet.json
```

## Design decisions worth knowing

**AI Service fallback.** `AIService` never throws out of a parsing/matching call. If no provider
is configured, or the HTTP call to the provider fails/times out, it transparently falls back to
deterministic keyword matching against a seed skills list. `ParseEngine` on the result tells you
which path was used ("OpenAI", "KeywordFallback", etc.) so this is observable, not silent.

**GenerateFeedback has an LLM path.** When an AI provider is configured, `GenerateFeedbackAsync`
calls the LLM for richer narrative summaries. If the call fails, it falls back to the deterministic
scoring logic. This ensures feedback generation works even without an AI provider.

**Keyword fallback extracts Education & Experience.** The fallback parser now uses regex patterns
to extract basic education entries (degree keywords + years) and experience entries (job title +
company patterns) from resume text, not just skills.

**Notification Service is provider-agnostic in its public surface.** `NotificationService` doesn't
talk to SendGrid/Twilio directly — it depends on `IEmailSender` / `ISmsSender` wrappers. This is
what makes `NotificationServiceTests` fast, deterministic unit tests instead of hitting real APIs
(SendGrid's client isn't mock-friendly on its own).

**Templates** are centralized in `NotificationTemplates.cs` so subject/body copy can be edited in
one place without touching service logic.

**AIScoreCalculator helper.** A convenience class that other team members' services can use to
calculate match scores and rank candidates without depending on the full `IAIService` interface.
It's registered in DI via `AddKaveeshaModule`.

## Wiring it into the main solution

1. Copy `Services/`, `DTOs/`, `Helpers/`, `Extensions/` into the main `RecruitmentAPI` Web project
   (same namespaces are used: `RecruitmentAPI.Services.AI`, `RecruitmentAPI.Services.Notification`,
   `RecruitmentAPI.Helpers`, etc.)
2. In `Program.cs`, add:
   ```csharp
   builder.Services.AddKaveeshaModule(builder.Configuration);
   ```
3. Merge the snippet from `appsettings.snippet.json` into `appsettings.json` / user-secrets
   (put real API keys in user-secrets or environment variables, never commit them).
4. Add `public partial class Program { }` at the bottom of `Program.cs` if it's not already
   there — `WebApplicationFactory<Program>` in `ApiIntegrationTests.cs` needs it.

## About `Contracts/TeamContracts.cs`

Sahan (Auth), Savindi (Candidate/Job/Application), Sobani (Interview/Feedback) haven't merged
their services yet, but the unit test checklist assigns their test files to Kaveesha.
`TeamContracts.cs` defines placeholder interfaces + DTOs that match the shapes agreed in the
master prompt, so:

- `AuthServiceTests.cs`, `CandidateServiceTests.cs`, `JobServiceTests.cs`,
  `ApplicationServiceTests.cs`, `InterviewServiceTests.cs`, `FeedbackServiceTests.cs` compile
  and run **today**, mocking those interfaces directly.
- Once each teammate merges their real interface + implementation, delete the corresponding
  section from `TeamContracts.cs`, update the `using` in that test file to point at the real
  namespace, and — ideally — stop mocking the service interface itself and instead mock its
  dependencies (`IUnitOfWork`, `IAIService`, `INotificationService`) so the test exercises real
  business logic, not just a mock echoing back what you told it to return. The mock-the-interface
  versions here are a placeholder scaffold, not a substitute for that.

`DatabaseIntegrationTests.cs` similarly uses a `TestUser`/`TestDbContext` stand-in — swap for the
real `RecruitmentAPI.Models.User` / `RecruitmentAPI.Data.ApplicationDbContext` once merged.

## Running the tests

```bash
cd Tests
dotnet restore
dotnet test
```

Note: `ApiIntegrationTests.cs` needs a project reference to the actual Web API project (for
`Program`) and won't compile standalone until that project exists in the solution — comment it
out or exclude it until then if you need the rest of the suite green.

## Coverage target

Checklist asks for >70% coverage. Run with:
```bash
dotnet test --collect:"XPlat Code Coverage"
# or with coverlet.msbuild:
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```
`AIService` and `NotificationService` (Kaveesha's own components) are covered close to 100% of
branches, including the provider-failure fallback path. The placeholder tests for teammates'
services will need re-authoring against real implementations to count meaningfully toward this
number — mocks-of-interfaces don't exercise real logic.
