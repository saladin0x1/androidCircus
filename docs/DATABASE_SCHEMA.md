# Database Schema Design

## Overview
Relational database schema for medical appointment management system.
**Recommended**: PostgreSQL or SQL Server

## Tables

### 1. Users (Base table for all user types)
```sql
Users
├── Id (PK, GUID)
├── Email (unique, indexed)
├── PasswordHash
├── PasswordSalt
├── FirstName
├── LastName
├── Phone
├── Role (enum: Patient, Doctor, Clerk)
├── IsActive (boolean)
├── CreatedAt (datetime)
├── UpdatedAt (datetime)
└── LastLoginAt (datetime)
```

**Indexes:**
- Email (unique)
- Role
- IsActive

---

### 2. Patients
```sql
Patients
├── Id (PK, GUID)
├── UserId (FK -> Users.Id)
├── DateOfBirth
├── Gender (enum: Male, Female, Other)
├── Address
├── City
├── PostalCode
├── EmergencyContactName
├── EmergencyContactPhone
├── BloodType (optional)
├── Allergies (text, optional)
├── MedicalConditions (text, optional)
├── InsuranceNumber (optional)
└── RegistrationDate (datetime)
```

**Indexes:**
- UserId (unique, FK)

---

### 3. Doctors
```sql
Doctors
├── Id (PK, GUID)
├── UserId (FK -> Users.Id)
├── Specialization
├── LicenseNumber (unique)
├── YearsOfExperience (int)
├── Education (text)
├── Biography (text, optional)
├── ConsultationFee (decimal, optional)
└── JoinedDate (datetime)
```

**Indexes:**
- UserId (unique, FK)
- Specialization
- LicenseNumber (unique)

---

### 4. Clerks
```sql
Clerks
├── Id (PK, GUID)
├── UserId (FK -> Users.Id)
├── EmployeeNumber (unique)
├── Department
└── HireDate (datetime)
```

**Indexes:**
- UserId (unique, FK)
- EmployeeNumber (unique)

---

### 5. Appointments
```sql
Appointments
├── Id (PK, GUID)
├── PatientId (FK -> Patients.Id)
├── DoctorId (FK -> Doctors.Id)
├── AppointmentDate (datetime, indexed)
├── DurationMinutes (int, default: 30)
├── Status (enum: Scheduled, Completed, Cancelled, NoShow)
├── Reason (text)
├── Notes (text, patient notes)
├── CreatedBy (FK -> Users.Id)
├── CreatedAt (datetime)
├── UpdatedAt (datetime)
└── CancelledAt (datetime, nullable)
```

**Status Enum:**
- Scheduled (default)
- Completed
- Cancelled
- NoShow

**Indexes:**
- PatientId
- DoctorId
- AppointmentDate
- Status
- Composite: (DoctorId, AppointmentDate)

**Constraints:**
- Unique: (DoctorId, AppointmentDate) - prevents double booking

---

### 6. MedicalRecords
```sql
MedicalRecords
├── Id (PK, GUID)
├── AppointmentId (FK -> Appointments.Id)
├── PatientId (FK -> Patients.Id)
├── DoctorId (FK -> Doctors.Id)
├── Diagnosis (text)
├── Prescription (text)
├── Notes (text)
├── Attachments (JSON, optional) // file paths or URLs
├── CreatedAt (datetime)
└── UpdatedAt (datetime)
```

**Indexes:**
- AppointmentId (unique, FK)
- PatientId
- DoctorId

---

### 7. DoctorAvailability
```sql
DoctorAvailability
├── Id (PK, GUID)
├── DoctorId (FK -> Doctors.Id)
├── DayOfWeek (enum: Monday-Sunday)
├── StartTime (time)
├── EndTime (time)
├── SlotDuration (int, minutes, default: 30)
├── IsActive (boolean)
└── EffectiveDate (date, optional) // for future schedule changes
```

**DayOfWeek Enum:**
- 0 = Sunday
- 1 = Monday
- ...
- 6 = Saturday

**Indexes:**
- DoctorId
- Composite: (DoctorId, DayOfWeek, IsActive)

---

### 8. RefreshTokens
```sql
RefreshTokens
├── Id (PK, GUID)
├── UserId (FK -> Users.Id)
├── Token (unique, indexed)
├── ExpiresAt (datetime)
├── CreatedAt (datetime)
├── RevokedAt (datetime, nullable)
└── IsRevoked (boolean, default: false)
```

**Indexes:**
- Token (unique)
- UserId
- ExpiresAt

---

### 9. AuditLog (Optional, for tracking changes)
```sql
AuditLog
├── Id (PK, GUID)
├── UserId (FK -> Users.Id)
├── Action (string) // CREATE, UPDATE, DELETE
├── EntityType (string) // Appointment, Patient, etc.
├── EntityId (GUID)
├── Changes (JSON) // old and new values
├── IpAddress (string)
└── Timestamp (datetime)
```

**Indexes:**
- UserId
- EntityType
- Timestamp

---

## Relationships Diagram

```
Users (1) ─────> (1) Patients
Users (1) ─────> (1) Doctors
Users (1) ─────> (1) Clerks
Users (1) ─────> (0..*) RefreshTokens
Users (1) ─────> (0..*) AuditLog

Patients (1) ──> (0..*) Appointments
Doctors (1) ───> (0..*) Appointments
Doctors (1) ───> (0..*) DoctorAvailability

Appointments (1) ─> (0..1) MedicalRecords

Patients (1) ──> (0..*) MedicalRecords
Doctors (1) ───> (0..*) MedicalRecords
```

---

## Entity Framework Core Models Structure

```
Domain/Entities/
├── User.cs (base class)
├── Patient.cs
├── Doctor.cs
├── Clerk.cs
├── Appointment.cs
├── MedicalRecord.cs
├── DoctorAvailability.cs
├── RefreshToken.cs
└── AuditLog.cs

Domain/Enums/
├── UserRole.cs
├── AppointmentStatus.cs
├── Gender.cs
└── DayOfWeek.cs (if not using built-in)
```

---

## Seeding Data (for POC testing)

### Admin/Clerk User
```
Email: clerk@clinic.com
Password: Clerk123!
Role: Clerk
```

### Sample Doctor
```
Email: doctor@clinic.com
Password: Doctor123!
Role: Doctor
Specialization: General Practitioner
```

### Sample Patient
```
Email: patient@clinic.com
Password: Patient123!
Role: Patient
```

### Doctor Availability (Sample)
```
Doctor: General Practitioner
Monday-Friday: 09:00 - 17:00
Slot Duration: 30 minutes
```

---

## Database Migrations Strategy

### Initial Migration
```
1. CreateUsersTable
2. CreatePatientsTable
3. CreateDoctorsTable
4. CreateClerksTable
5. CreateAppointmentsTable
6. CreateMedicalRecordsTable
7. CreateDoctorAvailabilityTable
8. CreateRefreshTokensTable
9. SeedInitialData
```

### Future Considerations
- Add Notifications table
- Add Payments table (if needed)
- Add Clinic/Branches table (multi-location support)
- Add Reviews/Ratings table
- Add Messages table (doctor-patient communication)

---

## Performance Considerations

### Indexes Strategy
- All foreign keys indexed automatically
- Composite indexes on frequently queried combinations
- Appointment queries: (DoctorId, AppointmentDate, Status)
- Patient lookups: Email, Phone (with full-text search if needed)

### Partitioning (Future)
- Partition Appointments by date (yearly/quarterly)
- Archive old MedicalRecords to separate table

### Caching Strategy
- Doctor availability (rarely changes)
- Doctor list with specializations
- User profile data (5-10 min cache)

---

## Backup Strategy

### Daily Backups
- Full database backup at midnight
- Transaction log backup every 6 hours

### Retention
- Daily backups: 30 days
- Weekly backups: 3 months
- Monthly backups: 1 year
