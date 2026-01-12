# UI Discovery - androidCircus Medical Clinic App

**Discovery Date:** 2025-01-11
**Commit:** aab9a2b25f312258325ef6670058968a7295e138
**Purpose:** Pure audit of existing UI layer - no changes, recommendations, or implementations

---

## 1. Layout XML Files

### Activity Layouts (9 files)

| File | Purpose | Key Components |
|------|---------|----------------|
| `activity_splash.xml` | Launch screen | ImageView (ic_medical), branding |
| `activity_signin.xml` | Login form | TextInputLayout (email/password), Button, ImageView (ic_medical) |
| `activity_signup.xml` | Registration form | TextInputLayout (name/email/password/role), ImageView (ic_medical) |
| `activity_home.xml` | Patient dashboard | RecyclerView (appointments), FloatingActionButton (add), header with profile/logout buttons |
| `activity_profile.xml` | User profile | ScrollView, EditText (name/phone), ImageView (ic_profile), update button |
| `activity_doctor_home.xml` | Doctor dashboard | CardViews (stats, quick access), RecyclerView (today's appointments), header |
| `activity_doctor_patients.xml` | Doctor's patient list | (not examined in detail) |
| `activity_doctor_agenda.xml` | Doctor's calendar/agenda | (not examined in detail) |
| `activity_clerk_home.xml` | Clerk dashboard | CardViews (stats), RecyclerView (appointments), header |
| `activity_create_appointment.xml` | New appointment form | TextInputLayout (doctor/date/reason), Button |
| `activity_patient_dossier.xml` | Patient details | TabLayout, ViewPager2 (fragments: history, notes, appointments) |

### Fragment Layouts (6 files)

| File | Purpose | Parent Activity | Key Components |
|------|---------|----------------|----------------|
| `fragment_patient_dossier.xml` | Patient details tab | PatientDossierActivity (ViewPager2) | CardViews (info, stats), RecyclerView (appointments history) |
| `fragment_patient_history.xml` | Patient appointment history | PatientDossierActivity | RecyclerView |
| `fragment_patient_notes.xml` | Patient clinical notes | PatientDossierActivity | (not examined) |
| `fragment_patients.xml` | Patient list (doctor view) | DoctorPatientsActivity | CardView (search), RecyclerView |
| `fragment_agenda.xml` | Doctor's agenda | DoctorAgendaActivity | CardView (date selector), RecyclerView |

### Item Layouts (2 files)

| File | Purpose | Used In |
|------|---------|---------|
| `item_appointment.xml` | Single appointment card | RecyclerView in activity_home, activity_doctor_home, etc. |
| `item_patient.xml` | Single patient card | RecyclerView in fragment_patients |

---

## 2. Icon Usage

### Vector Drawables in `res/drawable/` (6 files)

| File | Type | Size (viewport) | fillColor | Purpose |
|------|------|-----------------|-----------|---------|
| `ic_medical.xml` | Vector | 200x200 | #000000 (hardcoded) | Medical cross symbol |
| `ic_profile.xml` | Vector | 100x100 | #000000 (hardcoded) | User/person icon |
| `ic_architect.xml` | Vector | 245x256 | #000000 (hardcoded) | Architect blueprint (unused?) |
| `ic_construction.xml` | Vector | 200x200 | #000000 (hardcoded) | Construction worker (unused?) |
| `ic_launcher_background.xml` | Vector | - | Multiple colors | App launcher background |
| `ic_launcher_foreground.xml` | Vector | - | #FFFFFF | App launcher foreground |

### Raster Assets in `res/drawable/` (1 file)

| File | Type | Purpose |
|------|------|---------|
| `ic_my_patients.png` | PNG raster | Patients icon (used in doctor home) |

### Assets Directory
- **Status:** Does not exist at commit aab9a2b
- No `assets/icons/` folder present

### Android System Drawables Used

| System Drawable | Used In | Context |
|-----------------|---------|---------|
| `@android:drawable/ic_input_add` | activity_home.xml | FloatingActionButton (add appointment) |
| `@android:drawable/ic_menu_my_calendar` | item_appointment.xml, activity_doctor_home.xml, fragment_agenda.xml | Date/agenda display |
| `@android:drawable/ic_menu_search` | fragment_patients.xml | Search icon |
| `@android:drawable/ic_menu_myplaces` | fragment_patients.xml | Location icon |
| `@android:drawable/ic_menu_call` | fragment_patient_dossier.xml | Call button |
| `@android:drawable/ic_menu_info_details` | fragment_patient_dossier.xml | Info/details |
| `@android:drawable/ic_menu_more` | item_patient.xml | Options menu |
| `@android:drawable/btn_dropdown` | activity_create_appointment.xml | Dropdown button |

### Icon Reference Count in Layouts

| Icon | Reference Count |
|------|-----------------|
| `@android:drawable/ic_menu_my_calendar` | 5 instances |
| `@drawable/ic_medical` | 2 instances |
| `@drawable/ic_profile` | 1 instance |
| `@drawable/ic_my_patients` (PNG) | 1 instance |
| `@android:drawable/ic_input_add` | 1 instance |
| `@android:drawable/ic_menu_search` | 1 instance |
| `@android:drawable/ic_menu_myplaces` | 1 instance |
| `@android:drawable/ic_menu_call` | 1 instance |
| `@android:drawable/ic_menu_info_details` | 1 instance |
| `@android:drawable/ic_menu_more` | 1 instance |
| `@android:drawable/btn_dropdown` | 1 instance |

**Total drawable references: 17 instances**

---

## 3. UI Components in Use

### Navigation Pattern

**Type:** Explicit intent-based navigation (no Navigation Component, no Bottom Navigation, no Drawer)

**Flow:**
1. `SplashActivity` → `SignInActivity` / `SignUpActivity`
2. `SignInActivity` → `HomeActivity` (routes based on user role: Patient/Doctor/Clerk)
3. `HomeActivity` (Patient) → `ProfileActivity`, `CreateAppointmentActivity`
4. `DoctorHomeActivity` → `DoctorPatientsActivity`, `DoctorAgendaActivity`
5. `ClerkHomeActivity` → (not fully examined)

**Back navigation:** Text-based back buttons (← symbol) in headers, clickable TextViews with `?attr/selectableItemBackground`

### Activity/Fragment Architecture

**Activities with ViewPager2:**
- `PatientDossierActivity` - Uses ViewPager2 with 3 fragments (History, Notes, Dossier)

**Activities with RecyclerView:**
- `HomeActivity` - Appointments list
- `DoctorHomeActivity` - Today's appointments
- `DoctorPatientsActivity` - Patient list
- `ClerkHomeActivity` - Appointments list

**Fragments:**
- All fragments are child views of ViewPager2 (tab-based navigation)
- No standalone fragments with own navigation

### Layout Components

**CardViews** (`androidx.cardview.widget.CardView`):
- Used for: Stat cards, appointment items, patient items, quick access cards
- Corner radius: 8dp - 16dp
- Elevation: 2dp - 4dp
- Background: Always white (`#FFFFFF`)

**RecyclerView** (`androidx.recyclerview.widget.RecyclerView`):
- Used in: All list screens (appointments, patients)
- Orientation: Vertical
- Padding: Some lists use `paddingBottom` for FAB clearance

**TabLayout** (`com.google.android.material.tabs.TabLayout`):
- Used in: `PatientDossierActivity`
- Mode: Fixed
- Gravity: Fill
- Indicator color: `#000000`

**ViewPager2** (`androidx.viewpager2.widget.ViewPager2`):
- Used in: `PatientDossierActivity`
- Paired with TabLayout for tab navigation

**FloatingActionButton** (`com.google.android.material.floatingactionbutton.FloatingActionButton`):
- Used in: `HomeActivity` (patient)
- Position: Bottom-end
- Background tint: `#000000`
- Icon: `@android:drawable/ic_input_add`
- Content description: "Nouveau rendez-vous"

**CoordinatorLayout** (`androidx.coordinatorlayout.widget.CoordinatorLayout`):
- Used in: `activity_home.xml`, `activity_clerk_home.xml`
- Purpose: Host FAB with proper positioning

**NestedScrollView** (`androidx.core.widget.NestedScrollView`):
- Used in: `activity_doctor_home.xml`

### Form Components

**TextInputLayout** (NOT using Material Components):
- Forms use raw `EditText` with `android:background="@android:color/transparent"`
- Custom dividers using `View` with 1dp height
- No Material TextInputLayout found in current layouts

**Buttons:**
- Most buttons are `TextView` with `clickable="true"` and `?attr/selectableItemBackground`
- No Material Button components found
- Text-based navigation (Back, Profil, Déconnexion)

---

## 4. Theme and Color Setup

### Theme Definition (`themes.xml`)

**Parent Theme:**
```xml
Theme.MaterialComponents.Light.NoActionBar
```

**Theme Attributes:**
```xml
<!-- Primary -->
<color name="colorPrimary">#000000</color>
<color name="colorPrimaryVariant">#000000</color>
<color name="colorOnPrimary">#FFFFFF</color>

<!-- Secondary -->
<color name="colorSecondary">#000000</color>
<color name="colorSecondaryVariant">#000000</color>
<color name="colorOnSecondary">#FFFFFF</color>

<!-- Status Bar -->
<color name="android:statusBarColor">#FFFFFF</color>
<color name="android:windowLightStatusBar">true</color>
```

**Custom Style:**
```xml
<style name="TabTextAppearance" parent="TextAppearance.Design.Tab">
    <item name="android:textSize">14sp</item>
    <item name="android:textAllCaps">false</item>
    <item name="fontFamily">sans-serif-medium</item>
</style>
```

### Color Palette (`colors.xml`)

**Current colors.xml (mostly unused defaults):**
```xml
<purple_200>#FFBB86FC</purple_200>
<purple_500>#FF6200EE</purple_500>
<purple_700>#FF3700B3</purple_700>
<teal_200>#FF03DAC5</teal_200>
<teal_700>#FF018786</teal_700>
<black>#FF000000</black>
<white>#FFFFFFFF</white>
```

**Note:** These are template colors from project creation. The app uses hardcoded hex colors in layouts instead.

### Hardcoded Color Usage in Layouts

**Background colors:**
- `#FAFAFA` - Main background (light gray)
- `#FFFFFF` - Card/header backgrounds (white)
- `#F0F0F0` - Divider color
- `#E8E8E8` - Subtle divider

**Text colors:**
- `#000000` - Primary text (black)
- `#6B6B6B` - Secondary text (medium gray)
- `#757575` - Tertiary text
- `#9E9E9E` - Label/hint text
- `#9B9B9B` - Placeholder text
- `#B0B0B0` - Disabled text
- `#C62828` - Error/logout (red)
- `#424242` - Body text

**Accent colors:**
- `#E8F5E9` - Success background (light green)
- `#2E7D32` - Success text (green)
- `#6B6B6B` - Icon tint (gray)

**Typography:**
- `sans-serif-medium` - Most headers
- Default font family for body text
- Text sizes: 12sp - 32sp range

---

## 5. Java UI Classes

### Activities (13 files)

| Class | Purpose | Layout |
|-------|---------|--------|
| `SplashActivity` | Launch screen | activity_splash.xml |
| `SignInActivity` | Authentication | activity_signin.xml |
| `SignUpActivity` | Registration | activity_signup.xml |
| `HomeActivity` | Patient dashboard | activity_home.xml |
| `ProfileActivity` | User profile | activity_profile.xml |
| `CreateAppointmentActivity` | Appointment creation | activity_create_appointment.xml |
| `DoctorHomeActivity` | Doctor dashboard | activity_doctor_home.xml |
| `DoctorPatientsActivity` | Doctor's patient list | activity_doctor_patients.xml |
| `DoctorAgendaActivity` | Doctor's calendar | activity_doctor_agenda.xml |
| `ClerkHomeActivity` | Clerk dashboard | activity_clerk_home.xml |
| `PatientDossierActivity` | Patient details | activity_patient_dossier.xml |

### Adapters (3 files)

| Class | Purpose | Item Layout |
|-------|---------|-------------|
| `AppointmentsAdapter` | RecyclerView for appointments | item_appointment.xml |
| `PatientsAdapter` | RecyclerView for patients | item_patient.xml |
| `PatientDossierPagerAdapter` | ViewPager2 for patient tabs | fragment_patient_*.xml |

### Pager Adapters (1 file)

| Class | Purpose |
|-------|---------|
| `DoctorViewPagerAdapter` | ViewPager for doctor fragments |

---

## 6. UI Patterns Observed

### Header Pattern
All screens use consistent header structure:
```xml
<LinearLayout
    android:orientation="horizontal"
    android:padding="20dp"
    android:background="#FFFFFF"
    android:elevation="2dp">

    <!-- Title or Welcome Text -->
    <TextView android:textSize="18-20sp" android:textColor="#000000" />

    <!-- Action Buttons (Profile, Logout, Back) -->
    <TextView android:clickable="true" android:background="?attr/selectableItemBackground" />
</LinearLayout>
<View android:height="1dp" android:background="#F0F0F0" />
```

### Card Pattern
Most content cards follow this structure:
```xml
<androidx.cardview.widget.CardView
    app:cardCornerRadius="8-16dp"
    app:cardElevation="2dp">

    <LinearLayout
        android:orientation="vertical"
        android:padding="12-20dp">

        <!-- Content here -->
    </LinearLayout>
</androidx.cardview.widget.CardView>
```

### Empty State Pattern
Some screens use empty states with:
- Large icon (64dp) with `android:alpha="0.3"`
- Centered text below icon
- `android:visibility="gone"` toggled by Java

### Form Pattern
Forms use custom input styling:
- `EditText` with transparent background
- `View` as divider (1dp height, #E8E8E8 or #F0F0F0)
- No Material TextInputLayout
- Labels as separate `TextView` above input

---

## 7. String Resources (Partial)

**Key strings observed in layouts:**
- "Bienvenue" (Welcome)
- "Mes Rendez-vous" (My Appointments)
- "Profil" (Profile)
- "Déconnexion" (Logout)
- "Retour" (Back)
- "Espace Médecin" (Doctor Space)
- "Espace Secrétariat" (Clerk Space)
- "Dossier Patient" (Patient Record)
- "Nouveau rendez-vous" (New appointment)

---

## 8. Observations

### Design Aesthetic
- **Minimalist black & white** - Primary color is pure black (`#000000`)
- **High contrast** - White backgrounds, black text
- **Material Design Lite** - Uses Material Components (CardView, TabLayout, ViewPager2) but not Material inputs/buttons
- **Custom forms** - No Material TextInputLayout, using EditText + View dividers

### Icon Strategy
- **Mixed sources:**
  - Custom vector drawables (ic_medical, ic_profile)
  - Android system drawables (calendar, search, add, etc.)
  - One PNG raster (ic_my_patients)
- **Unused icons:** ic_architect, ic_construction (present but not referenced)
- **No assets folder** - All icons in res/drawable

### Navigation
- **No standard navigation component** - No BottomNavigationView, NavigationDrawer, or Navigation Component
- **Intent-based** - Explicit startActivity() calls
- **Tab-based** - Only in PatientDossierActivity (ViewPager2 + TabLayout)

### Component Usage
- **Heavy use of RecyclerView** - All list screens
- **CardView for content grouping** - Stats, appointments, patients
- **CoordinatorLayout** - Only for FAB hosting
- **ViewPager2** - Only for patient dossier tabs
- **No fragments** outside of ViewPager2 children

---

## Summary Statistics

- **Total layout files:** 19
  - Activities: 11
  - Fragments: 6
  - Items: 2
- **Total drawable resources:** 7 (6 vector + 1 PNG)
- **Total icon references in layouts:** 17
- **Activities:** 13
- **Fragments:** 6 (all ViewPager children)
- **Adapters:** 3
- **Navigation components:** 0 (no BottomNav, Drawer, or Navigation Component)
- **Theme:** MaterialComponents.Light.NoActionBar with custom colors
- **Primary color:** Black (#000000)
- **Background:** Light gray (#FAFAFA)

---

**End of Discovery Report**
