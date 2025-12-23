# API Endpoints Specification

## Base URL
```
Development: http://localhost:5000/api
Production: https://your-domain.com/api
```

## Authentication Endpoints

### POST /auth/register
Register a new user (Patient registration from app, or Clerk creating users)
```json
Request:
{
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "password": "string",
  "phone": "string",
  "role": "Patient", // Patient, Doctor, Clerk
  "dateOfBirth": "2000-01-01"
}

Response: 201 Created
{
  "success": true,
  "data": {
    "userId": "guid",
    "token": "jwt_token",
    "refreshToken": "refresh_token",
    "role": "Patient"
  }
}
```

### POST /auth/login
Login for all user types
```json
Request:
{
  "email": "string",
  "password": "string"
}

Response: 200 OK
{
  "success": true,
  "data": {
    "userId": "guid",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "role": "Patient|Doctor|Clerk",
    "token": "jwt_token",
    "refreshToken": "refresh_token"
  }
}
```

### POST /auth/refresh
Refresh expired token
```json
Request:
{
  "refreshToken": "string"
}

Response: 200 OK
{
  "success": true,
  "data": {
    "token": "new_jwt_token",
    "refreshToken": "new_refresh_token"
  }
}
```

### POST /auth/logout
Logout and invalidate tokens

---

## Patient Endpoints

### GET /patients/profile
Get logged-in patient's profile
**Auth**: Required (Patient role)

### PUT /patients/profile
Update patient profile
**Auth**: Required (Patient role)
```json
Request:
{
  "firstName": "string",
  "lastName": "string",
  "phone": "string",
  "address": "string",
  "emergencyContact": "string"
}
```

### GET /patients/appointments
Get patient's appointments (past and upcoming)
**Auth**: Required (Patient role)
```
Query params:
  - status: upcoming|past|all
  - page: int
  - pageSize: int
```

### GET /patients/medical-history
Get patient's medical records
**Auth**: Required (Patient role)

---

## Doctor Endpoints

### GET /doctors
Get list of all doctors (public or auth required)
```
Query params:
  - specialization: string
  - available: boolean
```

### GET /doctors/{doctorId}
Get doctor details and availability

### GET /doctors/me/appointments
Get logged-in doctor's appointments
**Auth**: Required (Doctor role)
```
Query params:
  - date: yyyy-MM-dd
  - status: scheduled|completed|cancelled
```

### GET /doctors/me/schedule
Get doctor's availability schedule
**Auth**: Required (Doctor role)

### PUT /doctors/me/schedule
Update doctor's availability
**Auth**: Required (Doctor role)
```json
Request:
{
  "dayOfWeek": "Monday",
  "startTime": "09:00",
  "endTime": "17:00",
  "slotDuration": 30 // minutes
}
```

### GET /doctors/me/patients
Get list of doctor's patients
**Auth**: Required (Doctor role)

### PUT /doctors/appointments/{appointmentId}/complete
Mark appointment as completed and add notes
**Auth**: Required (Doctor role)
```json
Request:
{
  "notes": "string",
  "diagnosis": "string",
  "prescription": "string"
}
```

---

## Appointment Endpoints

### POST /appointments
Create new appointment
**Auth**: Required (Patient or Clerk)
```json
Request:
{
  "patientId": "guid",
  "doctorId": "guid",
  "appointmentDate": "2024-12-25T10:00:00",
  "reason": "string",
  "notes": "string"
}

Response: 201 Created
{
  "success": true,
  "data": {
    "appointmentId": "guid",
    "appointmentDate": "2024-12-25T10:00:00",
    "doctor": {
      "id": "guid",
      "name": "Dr. Smith"
    },
    "patient": {
      "id": "guid",
      "name": "John Doe"
    },
    "status": "Scheduled"
  }
}
```

### GET /appointments/{appointmentId}
Get appointment details
**Auth**: Required (role-based access)

### PUT /appointments/{appointmentId}
Update appointment
**Auth**: Required (Patient or Clerk)
```json
Request:
{
  "appointmentDate": "2024-12-26T10:00:00",
  "reason": "string"
}
```

### DELETE /appointments/{appointmentId}
Cancel appointment
**Auth**: Required (Patient or Clerk)

### GET /appointments/available-slots
Get available time slots for a doctor
```
Query params:
  - doctorId: guid
  - date: yyyy-MM-dd
```

---

## Clerk Endpoints

### GET /clerk/dashboard
Get dashboard overview (stats)
**Auth**: Required (Clerk role)
```json
Response:
{
  "success": true,
  "data": {
    "todayAppointments": 15,
    "pendingAppointments": 5,
    "totalPatients": 150,
    "totalDoctors": 8
  }
}
```

### GET /clerk/appointments
Get all appointments (with filters)
**Auth**: Required (Clerk role)
```
Query params:
  - date: yyyy-MM-dd
  - doctorId: guid
  - patientId: guid
  - status: scheduled|completed|cancelled
  - page: int
  - pageSize: int
```

### GET /clerk/patients
Get all patients
**Auth**: Required (Clerk role)
```
Query params:
  - search: string (name, email, phone)
  - page: int
  - pageSize: int
```

### POST /clerk/patients
Register new patient
**Auth**: Required (Clerk role)

### GET /clerk/patients/{patientId}
Get patient full details
**Auth**: Required (Clerk role)

### PUT /clerk/patients/{patientId}
Update patient information
**Auth**: Required (Clerk role)

### GET /clerk/doctors
Get all doctors with details
**Auth**: Required (Clerk role)

### POST /clerk/reports/daily
Generate daily report
**Auth**: Required (Clerk role)
```
Query params:
  - date: yyyy-MM-dd
```

---

## Common Response Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (missing/invalid token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 409 | Conflict (e.g., appointment time already taken) |
| 500 | Internal Server Error |

## Pagination Format

All list endpoints support pagination:
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  }
}
```

## Error Response Format
```json
{
  "success": false,
  "error": {
    "code": "APPOINTMENT_CONFLICT",
    "message": "This time slot is already booked",
    "details": {
      "field": "appointmentDate",
      "suggestedSlots": [...]
    }
  }
}
```
