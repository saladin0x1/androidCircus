# Application de Gestion de Rendez-vous - Cabinet Médical

## Description

Cette application Android permet de gérer les rendez-vous dans un cabinet médical.
Elle est conçue pour organiser, planifier et suivre les rendez-vous des patients.

## Architecture

- **Backend**: .NET 8 Web API avec JWT Authentication
- **Frontend**: Android (Native Java) avec Retrofit
- **Database**: SQLite (via Entity Framework Core)
- **Deployment**: Docker + docker-compose

## Fonctionnalités Actuelles

### Backend (.NET API)
- [x] Authentication (JWT) - Login/Register
- [x] Role-based authorization (Patient, Doctor, Clerk)
- [x] Appointment management (Create, Read, Complete, Cancel)
- [x] Doctor listing
- [x] Clerk dashboard with statistics

### Android UI
- [x] User authentication (Sign in/Sign up)
- [x] Role-based navigation (Patient, Doctor, Clerk)
- [x] Patient: Create and view appointments
- [x] Doctor: View and complete appointments
- [x] Clerk: Dashboard with real-time statistics
- [x] Session management with JWT
- [x] Native notifications

---

## Fonctionnalités à Implémenter

### Backend API - Endpoints Manquants

#### Profile Management
- [ ] `GET /api/users/profile` - Fetch full user profile with role-specific details
- [ ] `PUT /api/users/profile` - Update user information (name, phone, email)
- [ ] `PUT /api/users/password` - Change password

#### Patient Management
- [ ] `GET /api/patients/{id}` - Get patient details (DOB, address, emergency contact)
- [ ] `PUT /api/patients/{id}` - Update patient profile

#### Doctor Management
- [ ] `GET /api/doctors/{id}` - Get full doctor profile (license, experience)
- [ ] `GET /api/doctors?specialization={spec}` - Filter doctors by specialization

#### Appointment Features
- [ ] `PUT /api/appointments/{id}` - Reschedule appointment (change date/time)
- [ ] `GET /api/appointments/history` - Get past appointments

#### Clerk/Admin Features
- [ ] `GET /api/clerk/patients` - List all patients
- [ ] `GET /api/clerk/doctors` - List all doctors (with management capabilities)
- [ ] `POST /api/clerk/doctors` - Add new doctor
- [ ] `PUT /api/users/{id}/active` - Activate/deactivate users

#### Security Fixes
- [ ] Add `[Authorize]` attribute to `DoctorsController` (currently unprotected)

---

### Android UI - Features à Ajouter

#### Profile Management
- [ ] Implement actual profile update in `ProfileActivity.java` (currently placeholder)
- [ ] Add patient-specific fields UI (DateOfBirth, Address, Emergency Contact)
- [ ] Create password change screen

#### Appointment Management
- [ ] Add appointment reschedule functionality
- [ ] Separate past appointments from upcoming

#### Clerk Features
- [ ] Patient management screen (view/search/manage patients)
- [ ] Doctor management screen (add/view/edit doctors)
- [ ] User activation/deactivation controls

---

## Installation

### Backend (.NET API)

```bash
cd ClinicAPI
docker-compose up --build
```

API accessible at: `http://localhost:8080`

### Android App

1. Cloner le dépôt et ouvrir le projet dans Android Studio :

```bash
git clone https://github.com/Ramiu88/androidCircus.git
cd androidCircus
```

2. Update API base URL in `RetrofitClient.java` if needed
3. Build and run on emulator or physical device

---

## Security Considerations

### Current Implementation
- Backend uses JWT for authentication with role-based claims
- Role-based authorization on most endpoints
- Client stores JWT token in SharedPreferences
- Role-based UI routing on Android

### Known Issues
- Client-side role switching is vulnerable on rooted devices (UI only, backend is protected)
- `DoctorsController` missing `[Authorize]` attribute
- Client can access UI screens they shouldn't see (but API blocks unauthorized actions)

### Recommendations
- Add `[Authorize]` to all controllers
- Implement client-side JWT validation before UI routing
- Audit all endpoints for proper authorization
- Add API-level logging for unauthorized access attempts

---

## Database Models

### User
- Email, Password, FirstName, LastName, Phone
- Role (Patient, Doctor, Clerk)
- IsActive status

### Patient
- DateOfBirth, Address
- EmergencyContactName, EmergencyContactPhone

### Doctor
- Specialization, LicenseNumber
- YearsOfExperience

### Appointment
- Patient, Doctor, AppointmentDate
- Reason, Notes, DoctorNotes
- Status (Scheduled, Completed, Cancelled)
