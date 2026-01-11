# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Medical appointment management POC (Proof of Concept) with three distinct user roles:
- **Patient**: Book, view, and cancel appointments
- **Doctor**: View assigned appointments and complete them with notes
- **Clerk**: Manage all appointments and register new patients

**Tech Stack**: Native Android (Java) frontend + .NET 8 Web API backend + SQLite database

## Architecture

### Backend (.NET API in `ClinicAPI/`)
- **Framework**: .NET 8 Web API with JWT authentication
- **Database**: SQLite via Entity Framework Core
- **Controllers**: `AuthController`, `AppointmentsController`, `DoctorsController`, `ClerkController`
- **Entry Point**: `ClinicAPI/src/API/Program.cs`
- **Data Layer**: Uses EF Core with `ApplicationDbContext` in `Data/` folder
- **DTOs**: Request/response models in `DTOs/` folder for API contracts

### Frontend (Android in `app/`)
- **Language**: Java (native Android)
- **Networking**: Retrofit 2 + OkHttp for REST API calls
- **Session**: JWT token stored in SharedPreferences via `SessionManager.java`
- **API Models**: POJOs in `app/src/main/java/com/example/myapplication/api/models/`
- **Role-based Routing**: `HomeActivity.java` routes to `PatientHomeActivity`, `DoctorHomeActivity`, or `ClerkHomeActivity` based on user role

### Authentication Flow
1. Android app calls `POST /api/auth/login` with email/password
2. Backend returns JWT token with user role claims
3. Client stores token in `SessionManager` (SharedPreferences)
4. All subsequent API calls include `Authorization: Bearer {token}` header
5. Backend validates JWT and enforces role-based authorization

## Common Commands

### Backend (.NET API)

```bash
# Run API locally (development)
cd ClinicAPI/src/API
dotnet run

# Run with specific URL binding (for Android WiFi testing)
dotnet run --urls="http://0.0.0.0:5000"

# Run with Docker (production-like)
cd ClinicAPI
docker-compose up --build
docker-compose up -d  # detached mode
docker-compose logs -f clinic-api  # view logs
docker-compose down

# Database migrations
cd ClinicAPI/src/API
dotnet ef migrations add MigrationName
dotnet ef database update

# Reset database (delete and recreate with seed data)
rm ClinicAPI/src/API/clinic.db*
dotnet run

# Build only
dotnet build
```

### Android

```bash
# Build APK
./gradlew assembleDebug

# Install on connected device
./gradlew installDebug

# Run tests
./gradlew test
./gradlew connectedAndroidTest
```

## Configuration

### API Base URL

**Critical**: The Android app's API base URL is in `app/src/main/java/com/example/myapplication/api/RetrofitClient.java:14`

Current configuration points to VPS at `http://26.10.1.235:8080/api/`

For local development, change to your machine's LAN IP:
```java
private static final String BASE_URL = "http://192.168.1.XXX:5000/api/";
```

Find your IP:
- **Mac/Linux**: `ifconfig | grep inet`
- **Windows**: `ipconfig`

### Test Accounts

All passwords: `Password123!`

| Role | Email | User ID |
|------|-------|---------|
| Patient | patient@clinic.com | 44444444-4444-4444-4444-444444444444 |
| Doctor | doctor@clinic.com | 55555555-5555-5555-5555-555555555555 |
| Clerk | clerk@clinic.com | 66666666-6666-6666-6666-666666666666 |

## API Endpoints

Base URL: `http://localhost:5000/api/` (or configured URL)

**Authentication**
- `POST /api/auth/login` - Login (returns JWT)
- `POST /api/auth/register` - Register new user

**Appointments**
- `GET /api/appointments` - List appointments (filtered by user role)
- `GET /api/appointments/{id}` - Get appointment details
- `POST /api/appointments` - Create appointment
- `PUT /api/appointments/{id}/complete` - Complete appointment (Doctor only)
- `DELETE /api/appointments/{id}` - Cancel appointment

**Doctors**
- `GET /api/doctors` - List all doctors

**Clerk Dashboard**
- `GET /api/clerk/dashboard` - Statistics (total appointments, patients, doctors)

## Android App Structure

```
app/src/main/java/com/example/myapplication/
├── api/
│   ├── RetrofitClient.java       # Singleton Retrofit instance
│   ├── ApiService.java           # Retrofit interface
│   ├── SessionManager.java       # JWT token storage
│   └── models/                   # POJOs for API requests/responses
├── SignInActivity.java           # Login screen
├── SignUpActivity.java           # Registration screen
├── SplashActivity.java           # Launch screen
├── HomeActivity.java             # Role-based router
├── PatientHomeActivity.java      # Patient dashboard
├── DoctorHomeActivity.java       # Doctor dashboard
├── ClerkHomeActivity.java        # Clerk dashboard
├── CreateAppointmentActivity.java
├── ProfileActivity.java
└── AppointmentsAdapter.java      # RecyclerView adapter
```

## Development Notes

### Role-Based Authorization
- Backend uses `[Authorize]` attribute with role policies on controllers
- JWT token contains `ClaimTypes.Role` with user's role
- Android routes to different Activity based on `SessionManager.getRole()`

### Network Configuration
Android requires HTTP cleartext traffic for local development:
- `AndroidManifest.xml` includes `INTERNET` permission
- `android:usesCleartextTraffic="true"` in `<application>` tag
- `res/xml/network_security_config.xml` allows HTTP for local IPs

### Database
- SQLite file: `ClinicAPI/src/API/clinic.db`
- Auto-created on first run with seed data
- Migrations managed via EF Core
- Docker volumes persist database in container at `/app/data/clinic.db`

### Known Issues
- `DoctorsController` missing `[Authorize]` attribute (security issue)
- Client-side role switching possible on rooted devices (UI only - backend protected)
- No refresh token mechanism (tokens expire after 24h)

## Testing

### Backend
- Swagger UI available at root URL when running: `http://localhost:5000`
- Use `/api/auth/login` to get JWT token
- Click "Authorize" button in Swagger and enter `Bearer {token}`
- Test all protected endpoints

### Android
- Test on physical device or emulator
- Ensure device and API server on same network for local testing
- Verify API base URL in `RetrofitClient.java`
- Test all three user roles (Patient, Doctor, Clerk)

## Documentation

See `docs/` folder for detailed documentation:
- `API_ARCHITECTURE.md` - Backend architecture and layers
- `API_ENDPOINTS.md` - Complete API endpoint reference
- `ANDROID_API_INTEGRATION.md` - Android integration guide
- `DATABASE_SCHEMA.md` - Database schema and models
- `VPS_DEPLOYMENT.md` - Production deployment instructions
