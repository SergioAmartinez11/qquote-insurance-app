# QQuote Insurance App

A full-stack vehicle insurance platform that lets customers get AI-powered quotes, manage policies, and file claims — all from a modern single-page application.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10 / ASP.NET Core, Clean Architecture |
| Database | PostgreSQL 17 (EF Core, code-first migrations) |
| AI | Anthropic Claude API (risk assessment on quotes) |
| Frontend | Angular 21, standalone components, zoneless CD |
| UI Library | PrimeNG 21, Tailwind CSS |
| Auth | JWT Bearer tokens |
| Container | Docker / Docker Compose |

---

## Architecture

```
qquote-insurance-app/
├── backend/
│   └── src/
│       ├── QQuote.Insurance.API            # Controllers, middleware, entry point
│       ├── QQuote.Insurance.Application    # Use cases, DTOs, service interfaces
│       ├── QQuote.Insurance.Domain         # Entities, value objects, domain events
│       └── QQuote.Insurance.Infrastructure # EF Core, identity, external services
├── frontend/
│   └── qquote-insurance-ui/               # Angular SPA
│       └── src/app/
│           ├── core/                      # Services, interceptors, guards
│           ├── layout/                    # App shell, sidebar, topbar
│           └── pages/                     # Feature pages (quotes, policies, claims)
└── docker/                                # Dockerfiles for API and frontend
```

### Domain Model

- **Customer** — registered user tied to an identity account
- **Quote** — vehicle insurance quote with AI-generated risk score and premium estimate
- **Policy** — active insurance policy converted from an accepted quote
- **Claim** — incident report filed against a policy, with a review lifecycle

---

## Features

### Quotes
- Request a quote by providing vehicle details and desired coverage type
- Claude AI assesses risk and returns a score, explanation, and monthly premium
- Convert an active quote into a policy with one click

### Policies
- View all active and cancelled policies
- See coverage details, premium, renewal date, and days remaining
- Navigate directly to quotes to get a new policy

### Claims
- Select a policy and view all claims filed against it
- File a new claim with an incident date and description
- Track claim status through a visual timeline: **Reported → Under Review → Resolved**

### Auth
- Register and log in with email/password
- JWT stored in `localStorage`, attached automatically via HTTP interceptor

---

## Running Locally

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Option A — Docker Compose (full stack)

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| API | http://localhost:5001 |
| Frontend | http://localhost:4201 |
| PostgreSQL | localhost:5432 |

### Option B — Local development

**1. Start the database**
```bash
docker compose up postgres -d
```

**2. Run the backend**
```bash
cd backend
dotnet run --project src/QQuote.Insurance.API
# API available at http://localhost:5001
```

**3. Run the frontend**
```bash
cd frontend/qquote-insurance-ui
npm install
npm start
# App available at http://localhost:4200
```

---

## API Endpoints

| Method | Path | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new customer |
| POST | `/api/auth/login` | Log in and receive a JWT |
| GET | `/api/auth/me` | Get current user info |
| GET | `/api/quotes` | List the current user's quotes |
| POST | `/api/quotes` | Request a new quote (triggers AI assessment) |
| GET | `/api/quotes/{id}` | Get quote details |
| GET | `/api/policies` | List the current user's policies |
| POST | `/api/policies/from-quote/{quoteId}` | Convert a quote into a policy |
| GET | `/api/claims/policy/{policyId}` | List claims for a policy |
| POST | `/api/claims` | File a new claim |

---

## Environment Configuration

The backend reads secrets from `appsettings.Development.json` (gitignored). Create the file at `backend/src/QQuote.Insurance.API/appsettings.Development.json` with:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=qquote_insurance;Username=qquote_user;Password=qquote_pass"
  },
  "Jwt": {
    "Key": "<your-jwt-secret>",
    "Issuer": "QQuoteInsurance",
    "Audience": "QQuoteInsuranceUsers"
  },
  "Claude": {
    "ApiKey": "<your-anthropic-api-key>"
  }
}
```
