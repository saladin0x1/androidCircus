# Clinic API - UI Developer Brief

**Base URL:** Configure in your app (e.g., `http://<YOUR_IP>:5000/api/`)

So the Android app can use:
- `http://localhost:5000/api/` (emulator)
- `http://192.168.1.XXX:5000/api/` (home LAN)
- `http://26.10.1.235:8080/api/` (school LAN)
- Whatever the network requires

**Authentication:** Bearer JWT token (all endpoints except `/api/auth/*` require auth)

---

## 1. Authentication (`/api/auth`)

| Endpoint | Method | Request | Response |
|----------|--------|---------|----------|
| `/login` | POST | `{email, password}` | `{success, data: {userId, firstName, lastName, email, role, token, roleSpecificId}}` |
| `/register` | POST | `{email, password, firstName, lastName, role}` | Same as login |
| `/forgot-password` | POST | `{email}` | `{success, data: {message}}` *(SMTP coming later)* |
| `/reset-password` | POST | `{email, newPassword, confirmPassword}` | `{success, data: {message}}` |
| `/refresh` | POST | *(empty)* | Returns new JWT token |
| `/health` | GET | - | `{status, timestamp}` |

**Note:** Use the `token` from login response in `Authorization: Bearer {token}` header

---

## 2. Appointments (`/api/appointments`)

| Endpoint | Method | Params | Description |
|----------|--------|--------|-------------|
| `/appointments` | GET | `?status`, `?date`, `?doctorId` | List appointments (filtered by role) |
| `/appointments` | POST | `{patientId, doctorId, appointmentDate, reason}` | Create appointment |
| `/appointments/{id}` | GET | - | Get appointment details |
| `/appointments/{id}` | PUT | `{appointmentDate}` | Reschedule appointment |
| `/appointments/{id}` | DELETE | - | Cancel appointment |
| `/appointments/{id}/status` | PUT | `{status}` | Update status (Doctor/Clerk only) |
| `/appointments/available-slots` | GET | `?doctorId`, `?date` | Get available time slots |

**Status values:** `Scheduled`, `Completed`, `Cancelled`, `NoShow`

---

## 3. Doctors (`/api/doctors`)

| Endpoint | Method | Access |
|----------|--------|--------|
| `/doctors` | GET | All authenticated |
| `/doctors/{id}` | GET | All authenticated |
| `/doctors` | POST | Clerk only |
| `/doctors/{id}` | PUT | Clerk only |
| `/doctors/{id}` | DELETE | Clerk only |
| `/doctors/dashboard` | GET | Doctor only |
| `/doctors/patients` | GET | Doctor only |
| `/doctors/agenda` | GET | Doctor only (`?startDate`, `?endDate`) |

---

## 4. Patients (`/api/patients`)

| Endpoint | Method | Access |
|----------|--------|--------|
| `/patients` | GET | Doctor/Clerk (`?search=name`) |
| `/patients` | POST | Clerk only |
| `/patients/{id}` | GET | Owner/Doctor/Clerk |
| `/patients/{id}` | PUT | Owner/Doctor/Clerk |
| `/patients/{id}` | DELETE | Clerk only |
| `/patients/{id}/notes` | GET | Doctor/Clerk |
| `/patients/{id}/notes` | PUT | Doctor/Clerk (`{notes}`) |
| `/patients/{id}/medical-history` | GET | Doctor/Clerk |

---

## 5. Clerk Dashboard (`/api/clerk`)

| Endpoint | Method | Response |
|----------|--------|----------|
| `/clerk/dashboard` | GET | `{todayAppointments, pendingAppointments, totalPatients, totalDoctors}` |
| `/clerk/dashboard/today` | GET | List of today's appointments |
| `/clerk/dashboard/pending` | GET | List of all scheduled appointments |

---

## 6. User Profile (`/api/users`)

| Endpoint | Method | Request/Response |
|----------|--------|------------------|
| `/users/me` | GET | Current user profile |
| `/users/me` | PUT | Update profile (`{firstName, lastName, phone, dateOfBirth, address, emergencyContactName, emergencyContactPhone}`) |
| `/users/me/password` | PUT | Change password (`{currentPassword, newPassword, confirmPassword}`) |

---

## Standard Response Format

```json
{
  "success": true|false,
  "data": { ... },
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message"
  }
}
```

---

## Common Error Codes

| Code | Description |
|------|-------------|
| `INVALID_TOKEN` | JWT token missing or invalid |
| `UNAUTHORIZED` | Wrong role for this endpoint |
| `NOT_FOUND` | Resource doesn't exist |
| `INVALID_ID` | GUID format invalid |
| `EMAIL_EXISTS` | Email already registered |
| `PASSWORD_MISMATCH` | Passwords don't match |
| `SLOT_UNAVAILABLE` | Time slot already booked |
| `ALREADY_COMPLETED` | Can't modify completed appointment |

---

## Test Accounts (Password: `Password123!`)

| Role | Email |
|------|-------|
| Patient | patient.jean.dupont@clinic.com |
| Doctor | doctor.martin.dupont@clinic.com |
| Clerk | clerk.claire.laurent@clinic.com |

---

## ID Fields

All IDs are **GUIDs (strings)**, not integers:
- `userId`, `patientId`, `doctorId`, `clerkId`, `appointmentId`

Example: `"af57425d-b237-4507-bc63-36f8d4e8d5b4"`

---

## Date Formats

- Send dates as ISO 8601: `2026-01-12T10:00:00Z`
- Or use date-only: `2026-01-12` (for queries)
