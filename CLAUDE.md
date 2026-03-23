# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

StreamingApp is a full-stack video streaming platform with HLS transcoding, real-time progress tracking, and object storage. Stack: .NET 8 + Angular 17 + PostgreSQL + Redis + MinIO + Hangfire + SignalR.

---

## Running the Application

### Docker Compose (recommended)

```bash
make up          # Start all services (PostgreSQL, Redis, MinIO, API, Frontend, Nginx)
make build       # Rebuild Docker images
make down        # Stop all services
make migrate     # Run EF Core migrations
make seed        # Seed initial data
make minio-setup # Configure MinIO bucket and access policy
make logs        # Tail API logs
```

**Service URLs:**
- Frontend: http://localhost:80
- API: http://localhost:5000
- MinIO console: http://localhost:9001
- Hangfire dashboard: http://localhost:5000/hangfire (local requests only)

### Local Development

```bash
# Backend
cd backend
dotnet watch run --project StreamingApp.API   # http://localhost:5000

# Frontend
cd frontend
ng serve --proxy-config proxy.conf.json       # http://localhost:4200
```

### Tests

```bash
# Frontend (Karma/Jasmine)
cd frontend && npm test

# Backend — no test projects exist yet; when adding, use xUnit + Moq
```

---

## Architecture

### Backend — Clean Architecture (.NET 8)

```
API (Controllers) → Application (Services, DTOs, Jobs) → Domain (Entities, Enums, Interfaces) → Infrastructure (EF Core, Repositories, External Services)
```

Dependency direction is always inward. Domain has no external dependencies. Infrastructure implements Domain interfaces.

### Frontend — Angular 17

Feature-based, lazy-loaded. Pattern: `core/` (singletons, guards, interceptors) → `features/` (lazy modules per route) → `shared/` (reusable components).

---

## Professional Development Conventions

These patterns are already established in the codebase. All new code must follow them.

### 1. Result Pattern (not exceptions for business logic)

All service methods return `Result<T>` or `Result` from `StreamingApp.Domain/Common/Result.cs`. Never throw exceptions for expected business outcomes (not found, unauthorized, validation failures). Exceptions are reserved for unexpected infrastructure failures caught by `ExceptionMiddleware`.

```csharp
// Correct
public async Task<Result<ContentDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    var content = await _contentRepo.GetWithEpisodesAsync(id, ct);
    if (content is null) return Result<ContentDto>.NotFound();
    return Result<ContentDto>.Success(MapToDto(content));
}

// Wrong — never throw for business logic
if (content is null) throw new NotFoundException(...);
```

Controllers extract the status code from the result:
```csharp
var result = await _service.GetByIdAsync(id, ct);
return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
```

### 2. DTOs are Records (immutable, validated)

Request and response DTOs are **C# records**, not classes. Input DTOs use `System.ComponentModel.DataAnnotations` for validation. Entities are never exposed through the API.

```csharp
// Request DTO
public record CreateContentDto(
    [Required] string Title,
    [Required] ContentType Type,
    [Required] Guid CategoryId,
    string? Description
);

// Response DTO — map from entity in the service layer
public record ContentDto(Guid Id, string Title, string Status, int ViewCount, ...);
```

### 3. Async/Await with CancellationToken everywhere

Every async method must:
- End with `Async` suffix
- Accept `CancellationToken ct = default` as the last parameter
- Pass `ct` down to all awaited calls (EF Core, HTTP, MinIO)

```csharp
// Correct
public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
{
    var user = await _userManager.FindByEmailAsync(dto.Email); // pass ct where supported
    ...
}
```

### 4. Repository Pattern

Use `IRepository<T>` for generic CRUD and `IContentRepository` (specialized) for domain-specific queries. Never put query logic in services — that belongs in the repository.

- `GetPagedAsync()`, `GetFeaturedAsync()`, `GetTrendingAsync()` → `IContentRepository`
- `GetByIdAsync()`, `AddAsync()`, `SaveChangesAsync()` → `IRepository<T>`

There is no Unit of Work pattern. Each service uses its own repositories, and `SaveChangesAsync()` is called on the repository.

### 5. Nullable Reference Types are enabled

The project uses `<Nullable>enable</Nullable>`. All new code must:
- Use `?` for nullable reference types explicitly
- Perform null checks before accessing nullable members
- Prefer `is null` / `is not null` over `== null` / `!= null`

### 6. Angular: Signals and Computed for State

State in services is managed with Angular 17 **signals**, not BehaviorSubjects or NgRx. Expose state as readonly signals; derive dependent values with `computed()`.

```typescript
// Correct
private readonly _user = signal<AuthUser | null>(this.loadFromStorage());
readonly user = this._user.asReadonly();
readonly isLoggedIn = computed(() => this._user() !== null);

// Wrong — don't introduce BehaviorSubject or NgRx for new state
private _user$ = new BehaviorSubject<AuthUser | null>(null);
```

Use `ChangeDetectionStrategy.OnPush` on all new components. Use functional guards (`CanActivateFn`), not class-based guards.

### 7. Logging via Serilog

Serilog is configured in `Program.cs` with console and daily rolling file sinks. Use structured logging (named placeholders, not string interpolation):

```csharp
// Correct — structured logging
_logger.LogInformation("Transcoding started for content {ContentId}", contentId);

// Wrong — string interpolation loses structure
_logger.LogInformation($"Transcoding started for content {contentId}");
```

Log at the appropriate level: `LogError` for caught exceptions and failures, `LogWarning` for recoverable issues, `LogInformation` for important state transitions (job started/finished, user registered).

### 8. Authorization

- `[Authorize]` for any authenticated endpoint
- `[Authorize(Roles = "Admin")]` for admin-only endpoints (upload, content management)
- Do not implement authorization logic inside services; keep it at the controller/route level
- The Hangfire dashboard is already protected by a local-only auth filter in `Program.cs`

### 9. TypeScript Strict Mode

`tsconfig.json` enforces `strict: true`, `noImplicitReturns`, `strictTemplates`. All new Angular code must compile without errors under these settings. Do not use `any` — use proper types or generics.

### 10. EF Core Model Configuration

Database relationships, indexes, and constraints are configured in `AppDbContext.OnModelCreating()` using the fluent API — not data annotations on entities. Add indexes for columns used in `WHERE` or `ORDER BY` clauses. Unique constraints (e.g., `(UserId, ContentId)` on ratings) are declared there too.

---

## Key Files

| File | Purpose |
|------|---------|
| `StreamingApp.Domain/Common/Result.cs` | Result pattern — the error handling contract |
| `StreamingApp.API/Program.cs` | DI registration, middleware pipeline, SignalR, Hangfire |
| `StreamingApp.API/Middleware/ExceptionMiddleware.cs` | Global exception → ProblemDetails handler |
| `StreamingApp.Application/Services/AuthService.cs` | Reference implementation: Result pattern + async + DTO mapping |
| `StreamingApp.Infrastructure/Repositories/ContentRepository.cs` | Reference for complex EF Core queries |
| `StreamingApp.Infrastructure/External/FfmpegService.cs` | Video transcoding to HLS (360p/720p/1080p) |
| `StreamingApp.Application/Jobs/TranscodingJob.cs` | Hangfire background job: download → transcode → upload to MinIO |
| `frontend/src/app/core/auth/auth.service.ts` | Signal-based state management reference |
| `frontend/src/app/features/player/player.component.ts` | Video.js + HLS + SignalR integration |
| `nginx/nginx.conf` | Routes: `/api/` → backend, `/hls/` → MinIO, `/` → Angular |

---

## Domain Model

| Entity | Key Notes |
|--------|-----------|
| `Content` | `TranscodingStatus`: Draft → Processing → Ready/Failed. HLS manifest URL stored after transcoding completes |
| `User` | Extends `IdentityUser`. `SubscriptionPlan` (Free/Basic/Premium), `IsAdmin` flag |
| `Episode` | Belongs to `Content` series only. Has `SeasonNumber`, `EpisodeNumber` |
| `WatchHistory` | Tracks per-user progress; updated during playback via SignalR |

---

## Adding New Features — Checklist

When implementing a new backend feature:
1. Define the domain entity or update an existing one in `StreamingApp.Domain/Entities/`
2. Add/update the repository interface in `StreamingApp.Domain/Interfaces/`
3. Implement the repository in `StreamingApp.Infrastructure/Repositories/`
4. Create request/response DTOs as **records** in `StreamingApp.Application/DTOs/`
5. Implement the service method returning `Result<T>`, with `CancellationToken ct = default`
6. Add the controller action, delegating all logic to the service
7. Register new services and repositories in `Program.cs`
8. Add EF Core migration: `dotnet ef migrations add <Name> --project StreamingApp.Infrastructure --startup-project StreamingApp.API`
   - Migrations ficam em `StreamingApp.Infrastructure/Migrations/`

When implementing a new Angular feature:
1. Generate a standalone component inside `features/<feature-name>/`
2. Add the route to `app.routes.ts` with `loadComponent` (lazy loading)
3. Manage state with signals in the feature's service; expose as readonly
4. Apply `ChangeDetectionStrategy.OnPush` to the component
5. Add route guard (`authGuard` or `adminGuard`) if the route requires authentication
