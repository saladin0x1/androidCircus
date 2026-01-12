# .NET API Architecture - Medical Appointment POC

## Project Overview
Full-stack POC for a medical appointment management system with:
- **Frontend**: Native Android app (Java) - 3 device deployment
- **Backend**: .NET Web API (REST)
- **Database**: SQL Server / PostgreSQL
- **Deployment**: DevOps pipeline ready

## User Roles

### 1. Patient
- Book appointments
- View their appointments
- Cancel appointments
- View their medical history
- Update profile information

### 2. Doctor
- View assigned appointments
- View patient information
- Manage availability/schedule
- Complete appointments (add notes)
- View appointment history

### 3. Clerk (Receptionist)
- Manage all appointments (CRUD)
- Register new patients
- Assign doctors to appointments
- View clinic schedule
- Generate reports

## Technology Stack

### Backend (.NET API)
```
- .NET 8 Web API
- Entity Framework Core
- SQL Server / PostgreSQL
- JWT Authentication
- Swagger/OpenAPI documentation
```

### Authentication Flow
```
1. Login endpoint returns JWT token
2. Token includes user role (Patient/Doctor/Clerk)
3. All subsequent requests include Bearer token
4. Role-based authorization on endpoints
```

## API Architecture Layers

### 1. Presentation Layer (Controllers)
- AuthController
- PatientController
- DoctorController
- AppointmentController
- ClerkController

### 2. Business Logic Layer (Services)
- AuthService
- PatientService
- DoctorService
- AppointmentService
- NotificationService

### 3. Data Access Layer (Repositories)
- Generic Repository Pattern
- Unit of Work Pattern
- Entity Framework Core

### 4. Data Models
- User (base for Patient/Doctor/Clerk)
- Patient
- Doctor
- Appointment
- MedicalRecord
- Availability

## Security Considerations

### Authentication
- JWT tokens with 24h expiration
- Refresh token mechanism
- Password hashing (BCrypt)

### Authorization
- Role-based access control
- Endpoint-level authorization
- Data isolation (patients see only their data)

### Data Protection
- HTTPS only
- Sensitive data encryption at rest
- CORS configuration for Android app

## API Response Format

### Success Response
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful"
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Error description"
  }
}
```

## Deployment Architecture

### Development
```
Android App (3 devices) -> .NET API (localhost/dev server) -> Database
```

### Production POC
```
Android App (3 devices)
    ↓ HTTPS
API Gateway / Reverse Proxy
    ↓
.NET API (Docker container)
    ↓
SQL Server / PostgreSQL (Docker container)
```

## Next Steps
1. Detail API endpoints specification
2. Design complete database schema
3. Plan DevOps pipeline (Docker, CI/CD)
4. Android app API integration plan
