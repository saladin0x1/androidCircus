# Icon Selection Plan - androidCircus Medical Clinic App

**Planning Date:** 2025-01-11
**Based on:** UI_DISCOVERY.md and UX_FLOWS.md
**Purpose:** Complete icon set specification - no implementation

---

## Icon Inventory Summary

**Current State:**
- 17 system drawable references (ic_menu_*, btn_dropdown)
- 4 custom vectors (2 unused)
- 1 PNG raster to convert
- Missing: Status indicators, empty states, proper navigation icons

**Target State:**
- Replace all system drawables with consistent Material Symbols
- Add medical branding icons
- Add status and empty state icons
- Clean up unused assets

---

## P1: Replace System Drawables (Critical)

These replace the inconsistent Android system drawables with properly sized, tintable vector icons.

### Navigation

| Icon | Purpose | Source | Filename | Replaces |
|------|---------|--------|----------|----------|
| Arrow Back | Back button, navigation | Material Symbols | `ic_arrow_back.xml` | Text "← Retour" |
| Home | Dashboard/home navigation | Material Symbols | `ic_home.xml` | (new) |
| Person | User profile, patient avatar | Material Symbols | `ic_person.xml` | `ic_profile.xml` |
| Logout | Sign out action | Material Symbols | `ic_logout.xml` | (new) |

### Actions

| Icon | Purpose | Source | Filename | Replaces |
|------|---------|--------|----------|----------|
| Add | Create new, FAB button | Material Symbols | `ic_add.xml` | `@android:drawable/ic_input_add` |
| Edit | Modify, update action | Material Symbols | `ic_edit.xml` | (new) |
| Delete | Remove action | Material Symbols | `ic_delete.xml` | (new) |
| Search | Search input | Material Symbols | `ic_search.xml` | `@android:drawable/ic_menu_search` |
| More Vert | Options menu | Material Symbols | `ic_more_vert.xml` | `@android:drawable/ic_menu_more` |
| Close | Dismiss, cancel | Material Symbols | `ic_close.xml` | (new) |
| Check | Confirm, done | Material Symbols | `ic_check.xml` | (new) |

### Content Icons

| Icon | Purpose | Source | Filename | Replaces |
|------|---------|--------|----------|----------|
| Calendar | Date, time, agenda | Material Symbols | `ic_calendar_month.xml` | `@android:drawable/ic_menu_my_calendar` |
| Event | Specific appointment/event | Material Symbols | `ic_event.xml` | (new) |
| Phone | Call, phone number | Material Symbols | `ic_phone.xml` | `@android:drawable/ic_menu_call` |
| Location | Place, facility | Material Symbols | `ic_location_on.xml` | `@android:drawable/ic_menu_myplaces` |
| Info | Information, details | Material Symbols | `ic_info.xml` | `@android:drawable/ic_menu_info_details` |
| Notes | Clinical notes, observations | Material Symbols | `ic_notes.xml` | (new) |
| History | Past records, timeline | Material Symbols | `ic_history.xml` | (new) |
| Expand | Dropdown indicator | Material Symbols | `ic_expand_more.xml` | `@android:drawable/btn_dropdown` |
| Group | Multiple patients, people list | Material Symbols | `ic_group.xml` | `ic_my_patients.png` |

### Form Inputs

| Icon | Purpose | Source | Filename | Replaces |
|------|---------|--------|----------|----------|
| Email | Email address field | Material Symbols | `ic_email.xml` | (new) |
| Lock | Password field | Material Symbols | `ic_lock.xml` | (new) |
| Visibility | Show password | Material Symbols | `ic_visibility.xml` | (new) |
| Visibility Off | Hide password | Material Symbols | `ic_visibility_off.xml` | (new) |

---

## P2: Missing Icons (High Priority)

These icons are currently missing but needed for complete functionality.

### Status Indicators

| Icon | Purpose | Source | Filename | Usage |
|------|---------|--------|----------|-------|
| Schedule | Pending/approval status | Material Symbols | `ic_schedule.xml` | "En attente" badge |
| Confirm Circle | Scheduled/confirmed | Material Symbols | `ic_check_circle.xml` | "Confirmé" badge |
| Cancel | Cancelled/rejected | Material Symbols | `ic_cancel.xml` | "Annulé" badge |
| Done | Completed appointment | Material Symbols | `ic_done_all.xml` | "Terminé" badge |
| Pending | Hourglass/pending | Material Symbols | `ic_hourglass_empty.xml` | Pending alt |
| Error | Failed, error state | Material Symbols | `ic_error.xml` | Error states |

### Role-Specific Icons

| Icon | Purpose | Source | Filename | Usage |
|------|---------|--------|----------|-------|
| Medical Services | Doctor badge, medical role | Material Symbols | `ic_medical_services.xml` | Doctor avatar |
| Admin Panel | Clerk/admin role | Material Symbols | `ic_admin_panel_settings.xml` | Clerk badge |
| Badge | Verified, official status | Material Symbols | `ic_verified.xml` | Verified user |

### Empty States

| Icon | Purpose | Source | Filename | Usage |
|------|---------|--------|----------|-------|
| Calendar Empty | No appointments | Material Symbols | `ic_event_available.xml` | Empty appointments list |
| People Empty | No patients | Material Symbols | `ic_person_off.xml` | Empty patients list |
| Search Empty | No search results | Material Symbols | `ic_search_off.xml` | Empty search results |
| Inbox Empty | No notifications | Material Symbols | `ic_notifications_none.xml` | Empty notifications list |

---

## P3: Medical Branding Icons (Low Priority)

App-specific medical icons from SVG sources. These should be sourced from medical icon libraries or custom SVGs.

### Core Medical Icons

| Icon | Purpose | Source | Filename | Usage |
|------|---------|--------|----------|-------|
| Medical Cross | Primary brand icon | Medical SVG | `ic_medical_cross.xml` | Replace `ic_medical.xml` (keep existing if good) |
| Stethoscope | Doctor indicator | Medical SVG | `ic_stethoscope.xml` | Doctor branding |
| Hospital | Facility, location | Medical SVG | `ic_hospital.xml` | Facility info |
| Clipboard | Notes, records | Medical SVG | `ic_clipboard.xml` | Medical records |
| Pill | Medication, prescription | Medical SVG | `ic_pill.xml` | Prescription info |
| Heart Rate | Vital signs | Medical SVG | `ic_heart_rate.xml` | Patient vitals |
| Patient Bed | Inpatient status | Medical SVG | `ic_patient_bed.xml` | Hospitalized patients |

### Notification Icons

| Icon | Purpose | Source | Filename | Usage |
|------|---------|--------|----------|-------|
| Notifications | Bell icon | Material Symbols | `ic_notifications.xml` | Notification center |
| Notifications Active | Unread indicator | Material Symbols | `ic_notifications_active.xml` | Unread badge |
| Announcement | Broadcast, info | Material Symbols | `ic_announcement.xml` | System announcements |

---

## Removals & Cleanup

### Delete Unused Vector Drawables

**Location:** `app/src/main/res/drawable/`

| File | Reason | Action |
|------|--------|--------|
| `ic_architect.xml` | Not referenced anywhere | DELETE |
| `ic_construction.xml` | Not referenced anywhere | DELETE |

### Convert PNG to Vector

**Location:** `app/src/main/res/drawable/`

| File | Replacement | Action |
|------|-------------|--------|
| `ic_my_patients.png` | `ic_group.xml` (Material Symbol) | DELETE PNG, use vector |

### System Drawable Replacements

After implementing new icons, these system drawable references will be removed from layouts:

| System Drawable | Replacement | Count |
|----------------|-------------|-------|
| `@android:drawable/ic_menu_my_calendar` | `ic_calendar_month.xml` | 5 |
| `@android:drawable/ic_input_add` | `ic_add.xml` | 1 |
| `@android:drawable/ic_menu_search` | `ic_search.xml` | 1 |
| `@android:drawable/ic_menu_myplaces` | `ic_location_on.xml` | 1 |
| `@android:drawable/ic_menu_call` | `ic_phone.xml` | 1 |
| `@android:drawable/ic_menu_info_details` | `ic_info.xml` | 1 |
| `@android:drawable/ic_menu_more` | `ic_more_vert.xml` | 1 |
| `@android:drawable/btn_dropdown` | `ic_expand_more.xml` | 1 |

**Total system drawables to replace:** 8 icons, 17 references

---

## Implementation Priority

### Phase 1: Critical Replacements (Do first)
1. Create P1 navigation icons (arrow_back, home, person, logout)
2. Create P1 action icons (add, edit, delete, search, more_vert)
3. Create P1 content icons (calendar_month, phone, location_on, group)
4. Replace all system drawable references in layouts
5. Delete unused vectors (ic_architect, ic_construction)
6. Delete ic_my_patients.png

**Impact:** Eliminates all inconsistent system drawables, establishes consistent icon language

### Phase 2: Add Missing Icons (Do second)
1. Create status indicator icons (schedule, check_circle, cancel, done_all)
2. Add empty state icons (event_available, person_off, search_off)
3. Implement role icons (medical_services, admin_panel_settings)
4. Update status badges in item_appointment.xml
5. Add empty states to all RecyclerView screens

**Impact:** Completes functionality, improves visual feedback

### Phase 3: Medical Branding (Do last)
1. Source or create medical SVG icons
2. Convert to vector drawables
3. Replace generic Material Symbols with medical-specific icons where appropriate
4. Update splash screen and auth screens with medical branding
5. Ensure consistent sizing and viewport

**Impact:** Establishes medical app identity, differentiates from generic apps

---

## Icon Sourcing Strategy

### Material Symbols (Primary Source)
- **URL:** https://fonts.google.com/icons
- **Why:** Free, consistent design language, proper vectors, multiple weights
- **Format:** Download as SVG, convert to Android vector drawable
- **Naming:** Use Material Symbol name (e.g., `calendar_month` → `ic_calendar_month.xml`)
- **Sizing:** Set viewport 24x24, remove width/height for flexibility

### Medical-Specific Icons
- **Sources:**
  - https://www.svgrepo.com/ (search "medical", "hospital", "doctor")
  - https://www.flaticon.com/ (medical icon packs)
  - Custom SVG creation
- **Requirements:**
  - SVG format with viewBox only (no hardcoded width/height)
  - Simple paths (avoid complex gradients, excessive detail)
  - Consistent stroke weight and style
  - Compatible with tinting (fillColor or strokeColor)
- **Conversion:** Same as Material Symbols - vector drawable with 24dp viewport

---

## Technical Specifications

### Vector Drawable Template

```xml
<vector xmlns:android="http://schemas.android.com/apk/res/android"
    android:width="24dp"
    android:height="24dp"
    android:viewportWidth="24"
    android:viewportHeight="24">
    <path
        android:fillColor="@color/colorOnSurface"
        android:pathData="..." />
</vector>
```

### Size Categories

| Context | Size | Usage |
|---------|------|-------|
| Small | 16dp | Inline icons, list items |
| Medium | 24dp | Buttons, inputs, default |
| Large | 32dp | FABs, hero icons |
| XLarge | 48dp+ | Empty states, branding |

### Tinting Strategy

All icons should use `@color/colorOnSurface` as base fillColor. Apply theme colors in layouts:

```xml
<ImageView
    android:src="@drawable/ic_calendar_month"
    app:tint="@color/colorPrimary" />
```

---

## File Organization

### Create Directory Structure

```
app/src/main/res/drawable/
├── ic_*.xml (all new vector icons)
├── assets/ (if keeping source SVGs)
│   └── icons/
│       ├── medical-*.svg
│       └── license.txt
```

### Naming Convention

- Prefix: `ic_`
- Material Symbols: Use symbol name (e.g., `ic_calendar_month.xml`)
- Custom medical icons: Descriptive name (e.g., `ic_stethoscope.xml`)
- Use snake_case
- Keep names under 30 characters

---

## Testing Checklist

After implementation, verify:

### Visual Consistency
- [ ] All icons use consistent stroke weight
- [ ] All icons properly align to 24dp grid
- [ ] Tinting works correctly on all icons
- [ ] No pixelation on any screen density

### Functionality
- [ ] All replaced icons maintain original meaning
- [ ] Status badges are distinguishable (color + icon)
- [ ] Empty states are helpful and clear
- [ ] Touch targets are at least 48dp for icon-only buttons

### Accessibility
- [ ] All icon-only buttons have contentDescription
- [ ] Icons with text labels have redundant contentDescription
- [ ] Color + icon used for status (not color alone)
- [ ] Touch targets meet minimum size requirements

### Asset Cleanup
- [ ] Unused vectors deleted
- [ ] PNG replaced with vector
- [ ] No system drawable references remain
- [ ] All new icons follow naming convention

---

## Summary Statistics

### Icons to Create
- **P1 (Critical):** 24 icons
- **P2 (Missing):** 18 icons
- **P3 (Medical):** 7+ icons
- **Total:** ~50 vector icons

### Icons to Delete
- **Unused vectors:** 2 files
- **PNG to replace:** 1 file
- **System drawables:** 8 types, 17 references

### Files to Modify
- **Layout files:** 15+ activities and items
- **Adapters:** 2 files (for status icons)
- **Java files:** Updates for new icon references

---

**End of Icon Plan**
