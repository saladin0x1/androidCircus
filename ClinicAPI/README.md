# Clinic API - Medical Appointment Management System

.NET 8 Web API for managing medical appointments with JWT authentication and role-based access control.

## Features

- JWT Authentication
- Role-based authorization (Patient, Doctor, Clerk)
- Swagger UI documentation
- SQLite database (easy local development)
- Docker support
- Seed data for testing

## Quick Start

### Option 1: Run with .NET CLI (Fastest)

```bash
cd src/API
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

API will be available at: `http://localhost:5000`
Swagger UI: `http://localhost:5000`

### Option 2: Run with Docker

```bash
# Build and run
docker-compose up --build

# Or run in detached mode
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

API will be available at: `http://localhost:5000`

## Test Accounts

All passwords: `Password123!`

| Role    | Email                   | Password       |
|---------|-------------------------|----------------|
| Patient | patient@clinic.com      | Password123!   |
| Doctor  | doctor@clinic.com       | Password123!   |
| Clerk   | clerk@clinic.com        | Password123!   |

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register new user
- `GET /api/auth/health` - Health check

### Appointments
- `GET /api/appointments` - Get appointments (filtered by role)
- `GET /api/appointments/{id}` - Get appointment details
- `POST /api/appointments` - Create appointment
- `PUT /api/appointments/{id}/complete` - Complete appointment (Doctor only)
- `DELETE /api/appointments/{id}` - Cancel appointment

## Testing with Swagger

1. Open browser to `http://localhost:5000`
2. Click "Authorize" button
3. Login using `/api/auth/login` endpoint
4. Copy the `token` from response
5. Click "Authorize" and enter: `Bearer {your-token}`
6. Test all endpoints

## Testing with curl

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"patient@clinic.com","password":"Password123!"}'
```

### Get Appointments (with token)
```bash
curl -X GET http://localhost:5000/api/appointments \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Create Appointment
```bash
curl -X POST http://localhost:5000/api/appointments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "patientId": "44444444-4444-4444-4444-444444444444",
    "doctorId": "55555555-5555-5555-5555-555555555555",
    "appointmentDate": "2024-12-30T10:00:00",
    "reason": "Checkup"
  }'
```

## Database

- **Type**: SQLite (for local development)
- **File**: `clinic.db` (created automatically)
- **Migrations**: Auto-applied on startup

### Reset Database

```bash
# Delete database file
rm src/API/clinic.db

# Run app (will recreate and seed)
dotnet run
```

## Environment Variables

Configure in `appsettings.json` or environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=clinic.db"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyForJWTMustBeAtLeast32CharactersLong!",
    "Issuer": "ClinicAPI",
    "Audience": "ClinicApp",
    "ExpirationHours": "24"
  }
}
```

## Project Structure

```
ClinicAPI/
├── src/API/
│   ├── Controllers/        # API endpoints
│   ├── Models/             # Database entities
│   ├── DTOs/               # Data transfer objects
│   ├── Services/           # Business logic
│   ├── Data/               # DbContext & migrations
│   ├── Program.cs          # App configuration
│   └── appsettings.json    # Configuration
├── Dockerfile              # Docker image config
├── docker-compose.yml      # Docker Compose config
└── README.md
```

## Deployment

### Local Network (for Android testing)

1. Find your local IP:
   ```bash
   # Mac/Linux
   ifconfig | grep inet

   # Windows
   ipconfig
   ```

2. Update Android app to use your IP:
   ```java
   String BASE_URL = "http://192.168.1.XXX:5000/api/";
   ```

3. Run API:
   ```bash
   dotnet run --urls="http://0.0.0.0:5000"
   ```

### Production (VPS)

See `VPS_DEPLOYMENT.md` for detailed instructions.

## Development

### Add New Migration

```bash
cd src/API
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Watch Mode (auto-reload)

```bash
dotnet watch run
```

## Troubleshooting

### Port 5000 already in use

```bash
# Change port in launchSettings.json or:
dotnet run --urls="http://localhost:5001"
```

### CORS errors from Android

- Check `UseCors()` is enabled in Program.cs
- Verify Android app has INTERNET permission
- Check network_security_config.xml allows HTTP

### JWT token expired

- Tokens expire after 24 hours (configurable)
- Login again to get new token

## Next Steps

1. Test all endpoints with Swagger
2. Update Android app to use this API
3. Deploy to VPS for remote access
4. Add more features (notifications, reports, etc.)

## Support

For issues, check:
- Swagger UI for endpoint details
- Application logs in console
- Docker logs: `docker-compose logs`
