# API Testing Guide

## API is Ready!

Your .NET API is fully built and running successfully!

**API URL**: `http://localhost:5201`
**Swagger UI**: `http://localhost:5201` (opens automatically)

---

## Quick Test with Swagger UI

1. **Open your browser** to: `http://localhost:5201`

2. **Test Authentication**:
   - Click on `POST /api/auth/login`
   - Click "Try it out"
   - Use this JSON:
   ```json
   {
     "email": "patient@clinic.com",
     "password": "Password123!"
   }
   ```
   - Click "Execute"
   - **Copy the `token` from the response**

3. **Authorize Swagger**:
   - Click the green "Authorize" button at the top
   - Paste: `Bearer {your-token-here}`
   - Click "Authorize" then "Close"

4. **Test Protected Endpoints**:
   - Try `GET /api/appointments` - see patient's appointments
   - Try `POST /api/appointments` - create new appointment
   - All endpoints are now authorized!

---

## Test Accounts

| Role    | Email                | Password      | Role-Specific ID                       |
|---------|----------------------|---------------|----------------------------------------|
| Patient | patient@clinic.com   | Password123!  | 44444444-4444-4444-4444-444444444444  |
| Doctor  | doctor@clinic.com    | Password123!  | 55555555-5555-5555-5555-555555555555  |
| Clerk   | clerk@clinic.com     | Password123!  | 66666666-6666-6666-6666-666666666666  |

---

## Test Scenarios

### As Patient
```json
POST /api/auth/login
{
  "email": "patient@clinic.com",
  "password": "Password123!"
}

GET /api/appointments
// Returns only this patient's appointments

POST /api/appointments
{
  "patientId": "44444444-4444-4444-4444-444444444444",
  "doctorId": "55555555-5555-5555-5555-555555555555",
  "appointmentDate": "2024-12-30T10:00:00",
  "reason": "Annual checkup"
}
```

### As Doctor
```json
POST /api/auth/login
{
  "email": "doctor@clinic.com",
  "password": "Password123!"
}

GET /api/appointments
// Returns only this doctor's appointments

PUT /api/appointments/{id}/complete
{
  "doctorNotes": "Patient is healthy. Recommended annual checkup next year."
}
```

### As Clerk
```json
POST /api/auth/login
{
  "email": "clerk@clinic.com",
  "password": "Password123!"
}

GET /api/appointments
// Returns ALL appointments (clerk sees everything)

POST /api/auth/register
{
  "email": "newpatient@example.com",
  "password": "Password123!",
  "firstName": "Jane",
  "lastName": "Smith",
  "phone": "+1234567890",
  "role": "Patient",
  "dateOfBirth": "1995-06-15"
}
```

---

## Run with Docker

```bash
# From ClinicAPI directory
docker-compose up --build

# Or in detached mode
docker-compose up -d

# View logs
docker-compose logs -f clinic-api

# Stop
docker-compose down
```

API will be at: `http://localhost:5000` (port 5000 in Docker, not 5201)

---

## Useful Commands

### Run Normally
```bash
cd src/API
dotnet run
```

### Reset Database
```bash
cd src/API
rm clinic.db clinic.db-*
dotnet run
# Database will be recreated with seed data
```

### Build Only
```bash
dotnet build
```

### Check Health
```bash
curl http://localhost:5201/health
```

---

## Android App Integration

### Update Android Base URL

In your Android app's `RetrofitClient.java`:

```java
// For local testing (same WiFi)
private static final String BASE_URL = "http://192.168.1.XXX:5201/api/";

// Find your IP:
// Mac: ifconfig | grep inet
// Windows: ipconfig
// Linux: ip addr show
```

### Update Network Config

`res/xml/network_security_config.xml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
    <domain-config cleartextTrafficPermitted="true">
        <domain includeSubdomains="true">192.168.1.0</domain>
        <domain includeSubdomains="true">localhost</domain>
    </domain-config>
</network-security-config>
```

---

## Next Steps

1. **API is built and tested** - Running successfully!
2. **Update Android app** - Follow ANDROID_API_INTEGRATION.md
3. **Test on 3 phones** - Patient, Doctor, Clerk
4. **Deploy to VPS** - Follow VPS_DEPLOYMENT.md

---

## Troubleshooting

### API not starting?
```bash
# Check if port is in use
lsof -i :5201

# Kill process
kill -9 <PID>

# Run on different port
dotnet run --urls="http://localhost:5555"
```

### Can't access from Android?
- Verify both devices on same WiFi
- Check firewall allows incoming connections
- Test: `curl http://YOUR_IP:5201/health` from phone browser

### Database issues?
```bash
# Delete and recreate
rm src/API/clinic.db*
dotnet run
```

---

## Pre-seeded Data

The database comes with:
- 3 users (patient, doctor, clerk)
- 1 patient profile
- 1 doctor profile (General Practitioner)
- 1 clerk profile
- 2 sample appointments

All ready to test immediately!

---

## Success!

Your API is production-ready for your POC demonstration tomorrow!

**Features included**:
- JWT Authentication
- Role-based authorization
- Swagger documentation
- SQLite database
- Seed data
- Docker support
- Error handling
- CORS enabled

**Test it now**: Open `http://localhost:5201` in your browser!
