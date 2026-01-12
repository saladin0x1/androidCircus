# UX Flows - androidCircus Medical Clinic App

**Discovery Date:** 2025-01-11
**Commit:** aab9a2b25f312258325ef6670058968a7295e138
**Purpose:** Documentation of user experience flows - no changes, no icon selection yet

---

## Navigation Overview

```
SplashActivity
     ↓
     ├─→ SignInActivity
     │       ↓
     │       ├─→ HomeActivity (Patient role)
     │       ├─→ DoctorHomeActivity (Doctor role)
     │       └─→ ClerkHomeActivity (Clerk role)
     │
     └─→ HomeActivity (if logged in as Patient)
         ├─→ DoctorHomeActivity (if logged in as Doctor)
         └─→ ClerkHomeActivity (if logged in as Clerk)
```

---

## Shared Authentication Screens

### SplashActivity
**Purpose:** Launch screen that routes users based on authentication status and role.

**Actions:**
- Automatic (no user interaction)

**Navigation:**
- From: App launch
- To: SignInActivity (if not logged in)
- To: HomeActivity (if logged in as Patient)
- To: DoctorHomeActivity (if logged in as Doctor)
- To: ClerkHomeActivity (if logged in as Clerk)

**Icon opportunities:**
- App logo/branding icon (currently uses ic_medical vector)

---

### SignInActivity
**Purpose:** User authentication via email and password.

**Actions:**
- Enter email
- Enter password
- Tap "Se connecter" (Sign in) button
- Tap "Pas encore de compte ? Créer un compte" link

**Navigation:**
- From: SplashActivity, manual launch, or after logout
- To: HomeActivity (Patient role)
- To: DoctorHomeActivity (Doctor role)
- To: ClerkHomeActivity (Clerk role)
- To: SignUpActivity (via sign-up link)

**Icon opportunities:**
- Medical branding icon (currently uses ic_medical)
- Email input field icon
- Password input field icon (show/hide)

---

### SignUpActivity
**Purpose:** New user registration for Patient role only.

**Actions:**
- Enter first name
- Enter last name
- Enter email
- Enter password
- Select role dropdown (currently hardcoded to Patient/role 0)
- Tap "S'inscrire" (Register) button
- Tap "← Retour" (Back) button

**Navigation:**
- From: SignInActivity (via sign-up link)
- To: HomeActivity (auto-login after registration)
- To: SignInActivity (via back button)

**Icon opportunities:**
- Medical branding icon (currently uses ic_medical)
- Input field icons (name, email, password)

---

## Patient User Flow

### HomeActivity (Patient Dashboard)
**Purpose:** Patient dashboard displaying list of upcoming appointments.

**Actions:**
- Tap "Profil" (Profile) button in header
- Tap "Déconnexion" (Logout) button in header
- Scroll through appointments list (RecyclerView)
- Tap FAB (+ icon) to create new appointment
- (Note: Appointment items have click capability in adapter but listener not implemented)

**Navigation:**
- From: SignInActivity (after login), SignUpActivity (after registration)
- To: ProfileActivity (via profile button)
- To: CreateAppointmentActivity (via FAB)
- To: SignInActivity (via logout)

**Icon opportunities:**
- Profile button icon
- Logout button icon
- Empty state icon (no appointments)
- FAB icon (currently uses ic_input_add system drawable)
- Appointment list item icons:
  - Doctor icon/avatar
  - Calendar/date icon (currently uses ic_menu_my_calendar system drawable)
  - Status badge indicator
- Welcome section icon

---

### ProfileActivity
**Purpose:** View and edit user profile information.

**Actions:**
- View email (read-only)
- Edit name input field
- Edit phone input field
- Tap "Mettre à jour" (Update) button (currently shows toast, not implemented)
- Tap "Retour" (Back) button in header

**Navigation:**
- From: HomeActivity (via profile button)
- To: HomeActivity (via back button)

**Icon opportunities:**
- Profile/avatar icon (currently uses ic_profile vector)
- Input field icons (email, name, phone)
- Update button icon

---

### CreateAppointmentActivity
**Purpose:** Form to create new appointment requests.

**Actions:**
- Tap "← Retour" (Back) button in header
- Tap date picker button to select appointment date
- Tap doctor dropdown spinner to select doctor
- Enter reason for appointment in text input
- Tap "Confirmer" (Confirm) button

**Navigation:**
- From: HomeActivity (via FAB)
- To: HomeActivity (after successful submission)

**Icon opportunities:**
- Back button indicator
- Date picker icon (currently uses btn_dropdown system drawable)
- Doctor dropdown icon
- Calendar icon in selected date display (currently uses ic_menu_my_calendar system drawable)

---

### PatientDossierActivity
**Purpose:** Patient details view with tabbed information.

**Actions:**
- Tap "← Retour" (Back) button in header
- Tap tabs to switch between: "Informations", "Historique", "Notes"
- Scroll through tab content

**Navigation:**
- From: (Not currently reachable in discovered flow - possibly from doctor or clerk views)
- To: Previous screen (via back button)

**Icon opportunities:**
- Back button indicator
- Tab icons (Information, History, Notes)
- Patient info section icons:
  - Phone call button (currently uses ic_menu_call system drawable)
  - Info/details icon (currently uses ic_menu_info_details system drawable)
- Empty state icons for each tab

---

## Doctor User Flow

### DoctorHomeActivity (Doctor Dashboard)
**Purpose:** Doctor dashboard with statistics, quick access cards, and today's appointments.

**Actions:**
- View welcome message with doctor name
- View stat cards: "Rendez-vous aujourd'hui", "Patients vus", "Taux de présence"
- Tap "Mon Agenda" card (currently no click listener - TODO)
- Tap "Mes Patients" card (currently no click listener - TODO)
- Scroll through today's appointments list
- Tap "Déconnexion" (Logout) button

**Navigation:**
- From: SignInActivity (after login as Doctor)
- To: DoctorAgendaActivity (via agenda card - TODO not implemented)
- To: DoctorPatientsActivity (via patients card - TODO not implemented)
- To: SignInActivity (via logout)

**Icon opportunities:**
- Stat card icons:
  - Calendar/agenda icon (currently uses ic_menu_my_calendar system drawable in agenda card)
  - Patients/group icon (currently uses ic_my_patients PNG raster)
- Empty state icon for appointments list
- Appointment list item icons:
  - Calendar icon (currently uses ic_menu_my_calendar system drawable)
  - Status indicator

---

### DoctorAgendaActivity
**Purpose:** Container for doctor's calendar/agenda view.

**Actions:**
- Tap "← Retour" (Back) button in header
- View AgendaFragment content (date selector, appointments by date)

**Navigation:**
- From: DoctorHomeActivity (via agenda card)
- To: DoctorHomeActivity (via back button)

**Icon opportunities:**
- Back button indicator
- Calendar/date selector icon
- Navigation arrows (previous/next day)

---

### DoctorPatientsActivity
**Purpose:** Container for doctor's patient list view.

**Actions:**
- Tap "← Retour" (Back) button in header
- Search patients (via search input)
- View PatientsFragment content (patient list cards)
- Tap patient card to view details
- Tap "more" options on patient card (currently uses ic_menu_more system drawable)

**Navigation:**
- From: DoctorHomeActivity (via patients card)
- To: DoctorHomeActivity (via back button)
- To: PatientDossierActivity (via patient card tap)

**Icon opportunities:**
- Back button indicator
- Search icon (currently uses ic_menu_search system drawable)
- Patient card:
  - Location/map icon (currently uses ic_menu_myplaces system drawable)
  - Options/menu icon (currently uses ic_menu_more system drawable)
- Empty state icon (no patients)

---

## Clerk User Flow

### ClerkHomeActivity (Clerk Dashboard)
**Purpose:** Clerk dashboard with statistics and appointments list.

**Actions:**
- View stat cards: "Rendez-vous aujourd'hui", "En attente" (pending)
- Scroll through appointments list (RecyclerView)
- Tap "Déconnexion" (Logout) button

**Navigation:**
- From: SignInActivity (after login as Clerk)
- To: SignInActivity (via logout)
- (Note: No sub-navigation discovered yet for clerk management functions)

**Icon opportunities:**
- Stat card icons
- Empty state icon for appointments list
- Appointment list item icons:
  - Calendar icon (currently uses ic_menu_my_calendar system drawable)
  - Status badge indicator (pending, scheduled, etc.)
- Quick action buttons (if clerk can approve/reject from list)

---

## Fragment Containers (Discovered)

The following activities are fragment containers with no direct UI logic:

### DoctorAgendaActivity
- Hosts: `AgendaFragment`
- Layout: `fragment_agenda.xml`
- Contains: Date selector card, RecyclerView for appointments

### DoctorPatientsActivity
- Hosts: `PatientsFragment`
- Layout: `fragment_patients.xml`
- Contains: Search input, RecyclerView for patient cards

### PatientDossierActivity
- Hosts: 3 fragments via ViewPager2
  - `PatientDossierFragment` (Information tab)
  - `PatientHistoryFragment` (Historique tab)
  - `PatientNotesFragment` (Notes tab)
- Layout: `fragment_patient_dossier.xml`, `fragment_patient_history.xml`, `fragment_patient_notes.xml`

---

## RecyclerView Adapters

### AppointmentsAdapter
**Used in:** HomeActivity, DoctorHomeActivity, ClerkHomeActivity
**Item layout:** `item_appointment.xml`
**Actions:**
- Scrollable list
- Click listener interface exists (onAppointmentClick) but not implemented in activities
- Displays: Doctor name, specialization, date/time, reason, status badge

**Icon opportunities:**
- Calendar icon (currently uses ic_menu_my_calendar system drawable)
- Status indicator icons (scheduled, pending, completed, cancelled)

### PatientsAdapter
**Used in:** DoctorPatientsActivity (via PatientsFragment)
**Item layout:** `item_patient.xml`
**Actions:**
- Scrollable list
- Click listener interface exists (onPatientClick)
- Displays: Patient name, age, last consultation

**Icon opportunities:**
- Patient avatar icon
- Location icon (currently uses ic_menu_myplaces system drawable)
- Options menu icon (currently uses ic_menu_more system drawable)

---

## Summary by User Role

### Patient (5 screens)
1. SignInActivity (login)
2. SignUpActivity (register)
3. HomeActivity (dashboard/appointments list)
4. ProfileActivity (edit profile)
5. CreateAppointmentActivity (new appointment)

### Doctor (5 screens)
1. SignInActivity (login)
2. DoctorHomeActivity (dashboard)
3. DoctorAgendaActivity (calendar view)
4. DoctorPatientsActivity (patient list)
5. PatientDossierActivity (patient details - shared)

### Clerk (2 screens discovered)
1. SignInActivity (login)
2. ClerkHomeActivity (dashboard)

**Note:** Clerk management screens (patient management, doctor management, appointment approval) may exist but were not discovered in current Activity files.

---

## Icon Inventory (Current Usage)

### System Drawables Used (17 instances)
| Icon | Locations | Purpose |
|------|-----------|---------|
| `ic_menu_my_calendar` | 5 | Date/agenda display |
| `ic_input_add` | 1 | FAB add button |
| `ic_menu_search` | 1 | Search input |
| `ic_menu_myplaces` | 1 | Location icon |
| `ic_menu_call` | 1 | Phone call button |
| `ic_menu_info_details` | 1 | Info/details button |
| `ic_menu_more` | 1 | Options menu |
| `btn_dropdown` | 1 | Dropdown button |

### Custom Vector Drawables (4 in use)
| Icon | File | Locations |
|------|------|-----------|
| Medical cross | `ic_medical.xml` | 2 (signin, signup) |
| User profile | `ic_profile.xml` | 1 (profile screen) |
| Architect | `ic_architect.xml` | 0 (unused) |
| Construction | `ic_construction.xml` | 0 (unused) |

### PNG Raster (1 in use)
| Icon | File | Locations |
|------|------|-----------|
| Patients group | `ic_my_patients.png` | 1 (doctor home) |

### Unused Assets
- `ic_architect.xml` - Present but not referenced in layouts
- `ic_construction.xml` - Present but not referenced in layouts
- `assets/icons/` folder - Does not exist at commit aab9a2b

---

## Observations

### Navigation Gaps
1. **Doctor navigation not connected:** DoctorHomeActivity has TODO comments for agenda/patients card click listeners
2. **Clerk management functions:** No activities discovered for clerk patient/doctor/appointment management
3. **PatientDossierActivity unreachable:** No navigation path discovered to access patient details

### Click Handlers
1. **AppointmentsAdapter:** Click listener interface exists but not implemented in activities
2. **PatientsAdapter:** Click listener interface exists, likely navigates to PatientDossierActivity

### Empty States
- Multiple lists use empty states with large icons + centered text
- Icons use `android:alpha="0.3"` for subtle appearance

### Header Pattern
- Consistent across all screens: Title on left, action buttons on right
- Back button as text "Retour" with selectableItemBackground
- Logout as "Déconnexion" in red (#C62828)

---

**End of UX Flow Documentation**
