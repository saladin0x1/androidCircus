# API Endpoints Reference

**Base URL:** `http://26.10.1.235:8080/api/` (or your local IP)

**Headers:**
```
Content-Type: application/json
Authorization: Bearer {token}  // Required for all endpoints except /auth/login and /auth/register
```

---

## Standard Response Wrapper

All endpoints return this structure:

```json
{
  "success": true|false,
  "data": { ... },           // present when success=true
  "error": {                 // present when success=false
    "code": "ERROR_CODE",
    "message": "Human readable error message"
  }
}
```

---

## Authentication

### `POST /api/auth/login`
Login with email/password

**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "role": "Patient|Doctor|Clerk",
    "token": "jwt_token_string",
    "roleSpecificId": "guid"
  }
}
```

**Errors:** `401 Unauthorized`, `500 Server Error`

---

### `POST /api/auth/register`
Register a new user

**Request:**
```json
{
  "email": "string (required)",
  "password": "string (min 6 chars, required)",
  "firstName": "string (required)",
  "lastName": "string (required)",
  "phone": "string (optional)",
  "role": "Patient|Doctor|Clerk (required)",
  "dateOfBirth": "datetime (optional for Patient)",
  "specialization": "string (optional for Doctor)"
}
```

**Response (201 Created):** Same as login response

**Errors:** `400 Bad Request`, `500 Server Error`

---

### `GET /api/auth/health`
Health check (no auth required)

**Response (200 OK):**
```json
{ "status": "Healthy", "timestamp": "datetime" }
```

---

## Doctors

### `GET /api/doctors`
List all doctors

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Dr. John Doe",
      "specialization": "Cardiology"
    }
  ]
}
```

---

## Patients

### `GET /api/patients/{id}`
Get patient details

**Auth:** `Doctor`, `Clerk` (any), `Patient` (own only)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "phone": "string",
    "dateOfBirth": "datetime|null",
    "address": "string|null",
    "emergencyContactName": "string|null",
    "emergencyContactPhone": "string|null",
    "doctorNotes": "string|null",
    "registrationDate": "datetime"
  }
}
```

**Errors:** `400`, `401`, `403`, `404`

---

### `GET /api/patients/{id}/notes`
Get doctor's notes for a patient

**Auth:** `Doctor`, `Clerk` only

**Response (200 OK):**
```json
{ "success": true, "data": "Doctor's notes text..." }
```

---

### `PUT /api/patients/{id}/notes`
Update doctor's notes

**Auth:** `Doctor` only

**Request:**
```json
{ "notes": "string" }
```

**Response (200 OK):**
```json
{ "success": true, "data": "Updated notes..." }
```

---

## Appointments

### `GET /api/appointments`
List appointments (filtered by user role)

**Auth:** All authenticated users

- **Patient**: sees own appointments only
- **Doctor**: sees assigned appointments only
- **Clerk**: sees all appointments

**Query Parameters:**
- `status` (optional): Filter by status (`Scheduled`, `Completed`, `Cancelled`, `NoShow`)

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "patientId": "guid",
      "doctorId": "guid",
      "appointmentDate": "datetime (ISO 8601)",
      "reason": "string|null",
      "notes": "string|null",
      "doctorNotes": "string|null",
      "status": "Scheduled|Completed|Cancelled|NoShow",
      "patientName": "John Doe",
      "doctorName": "Dr. Jane Smith",
      "doctorSpecialization": "Cardiology"
    }
  ]
}
```

**Errors:** `400`, `401`, `404`

---

### `GET /api/appointments/{id}`
Get specific appointment details

**Auth:** All authenticated users (role-based access same as list)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "patientId": "guid",
    "doctorId": "guid",
    "appointmentDate": "datetime",
    "reason": "string|null",
    "notes": "string|null",
    "doctorNotes": "string|null",
    "status": "Scheduled|Completed|Cancelled|NoShow",
    "patientName": "John Doe",
    "doctorName": "Dr. Jane Smith",
    "doctorSpecialization": "Cardiology"
  }
}
```

**Errors:** `400`, `401`, `403`, `404`

---

### `POST /api/appointments`
Create a new appointment

**Auth:** `Patient`, `Clerk` only

**Request:**
```json
{
  "patientId": "guid (required)",
  "doctorId": "guid (required)",
  "appointmentDate": "datetime (ISO 8601, required)",
  "reason": "string (optional)",
  "notes": "string (optional)"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "patientId": "guid",
    "doctorId": "guid",
    "appointmentDate": "datetime",
    "reason": "string|null",
    "notes": "string|null",
    "doctorNotes": "string|null",
    "status": "Scheduled",
    "patientName": "John Doe",
    "doctorName": "Dr. Jane Smith",
    "doctorSpecialization": "Cardiology"
  }
}
```

**Errors:**
- `400 Bad Request` - Invalid IDs, or already cancelled/completed
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Patient creating for another patient
- `404 Not Found` - Patient or doctor not found

---

### `PUT /api/appointments/{id}/complete`
Complete an appointment (add doctor notes)

**Auth:** `Doctor` only (assigned doctor)

**Request:**
```json
{
  "doctorNotes": "string (optional)"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "patientId": "guid",
    "doctorId": "guid",
    "appointmentDate": "datetime",
    "reason": "string|null",
    "notes": "string|null",
    "doctorNotes": "string",
    "status": "Completed",
    "patientName": "John Doe",
    "doctorName": "Dr. Jane Smith",
    "doctorSpecialization": "Cardiology"
  }
}
```

**Errors:**
- `400 Bad Request` - Invalid ID
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not the assigned doctor
- `404 Not Found` - Appointment not found

---

### `DELETE /api/appointments/{id}`
Cancel an appointment

**Auth:** `Patient` (own only), `Clerk` (any)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "message": "Appointment cancelled successfully",
    "appointmentId": "guid"
  }
}
```

**Errors:**
- `400 Bad Request` - Invalid ID, already cancelled, or already completed
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Patient cancelling another patient's appointment
- `404 Not Found` - Appointment not found

---

## Clerk Dashboard

### `GET /api/clerk/dashboard`
Get dashboard statistics

**Auth:** `Clerk` only

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "todayAppointments": 5,
    "pendingAppointments": 12,
    "totalPatients": 50,
    "totalDoctors": 4
  }
}
```

**Errors:** `401`, `403`

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Missing/invalid token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 500 | Server Error - Internal error |

---

## Role-Based Access Matrix

| Endpoint | Patient | Doctor | Clerk | No Auth |
|----------|---------|--------|-------|---------|
| POST /auth/login | ✅ | ✅ | ✅ | ✅ |
| POST /auth/register | ✅ | ✅ | ✅ | ✅ |
| GET /auth/health | ✅ | ✅ | ✅ | ✅ |
| GET /doctors | ✅ | ✅ | ✅ | ❌ |
| GET /patients/{id} | own | all | all | ❌ |
| GET /patients/{id}/notes | ❌ | ✅ | ✅ | ❌ |
| PUT /patients/{id}/notes | ❌ | ✅ | ❌ | ❌ |
| GET /appointments | own | assigned | all | ❌ |
| GET /appointments/{id} | own | assigned | all | ❌ |
| POST /appointments | ✅ | ❌ | ✅ | ❌ |
| PUT /appointments/{id}/complete | ❌ | ✅* | ❌ | ❌ |
| DELETE /appointments/{id} | own | ❌ | ✅ | ❌ |
| GET /clerk/dashboard | ❌ | ❌ | ✅ | ❌ |

*Assigned doctor only
