#!/bin/bash

# Clinic API - Test All Endpoints
# Usage: ./API_TEST_GUIDE.sh
# Make sure the API is running first!

BASE_URL="http://localhost:5000/api"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=========================================="
echo "  Clinic API - Endpoint Tester"
echo "  Base URL: $BASE_URL"
echo "=========================================="
echo ""

# Helper function to print section header
print_section() {
    echo -e "\n${YELLOW}========== $1 ==========${NC}"
}

# Helper function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local token=$3
    local data=$4

    echo -e "\n${GREEN}Testing: $method $endpoint${NC}"

    if [ -n "$data" ]; then
        curl -s -X "$method" "$BASE_URL$endpoint" \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $token" \
            -d "$data" | python3 -m json.tool 2>/dev/null || echo "Request failed"
    else
        curl -s -X "$method" "$BASE_URL$endpoint" \
            -H "Authorization: Bearer $token" \
            | python3 -m json.tool 2>/dev/null || echo "Request failed"
    fi
}

# First, login and get tokens
print_section "1. AUTHENTICATION - Login"

echo -e "\n${GREEN}Login as PATIENT${NC}"
PATIENT_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"patient.jean.dupont@clinic.com","password":"Password123!"}')
PATIENT_TOKEN=$(echo $PATIENT_RESPONSE | python3 -c "import sys, json; print(json.load(sys.stdin)['data']['token'])" 2>/dev/null)
echo "Token: ${PATIENT_TOKEN:0:50}..."

echo -e "\n${GREEN}Login as DOCTOR${NC}"
DOCTOR_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"doctor.martin.dupont@clinic.com","password":"Password123!"}')
DOCTOR_TOKEN=$(echo $DOCTOR_RESPONSE | python3 -c "import sys, json; print(json.load(sys.stdin)['data']['token'])" 2>/dev/null)
echo "Token: ${DOCTOR_TOKEN:0:50}..."

echo -e "\n${GREEN}Login as CLERK${NC}"
CLERK_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"clerk.claire.laurent@clinic.com","password":"Password123!"}')
CLERK_TOKEN=$(echo $CLERK_RESPONSE | python3 -c "import sys, json; print(json.load(sys.stdin)['data']['token'])" 2>/dev/null)
echo "Token: ${CLERK_TOKEN:0:50}..."

# Test Auth endpoints
print_section "2. AUTH ENDPOINTS"

echo -e "\n${GREEN}POST /auth/forgot-password${NC}"
curl -s -X POST "$BASE_URL/auth/forgot-password" \
    -H "Content-Type: application/json" \
    -d '{"email":"test@example.com"}' | python3 -m json.tool

echo -e "\n${GREEN}POST /auth/reset-password${NC}"
curl -s -X POST "$BASE_URL/auth/reset-password" \
    -H "Content-Type: application/json" \
    -d '{"email":"patient.jean.dupont@clinic.com","newPassword":"NewPass123!","confirmPassword":"NewPass123!"}' | python3 -m json.tool

echo -e "\n${GREEN}POST /auth/refresh${NC}"
curl -s -X POST "$BASE_URL/auth/refresh" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $PATIENT_TOKEN" \
    -d '{}' | python3 -m json.tool

echo -e "\n${GREEN}GET /auth/health${NC}"
curl -s -X GET "$BASE_URL/auth/health" | python3 -m json.tool

# Test Appointments endpoints
print_section "3. APPOINTMENTS (Patient)"

echo -e "\n${GREEN}GET /appointments (as Patient)${NC}"
curl -s -X GET "$BASE_URL/appointments" \
    -H "Authorization: Bearer $PATIENT_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /appointments/available-slots${NC}"
curl -s -X GET "$BASE_URL/appointments/available-slots?doctorId=f066397f-db0d-443f-a7f7-eed9751b67f0&date=2026-01-13" \
    -H "Authorization: Bearer $PATIENT_TOKEN" | python3 -m json.tool

# Test Doctors endpoints
print_section "4. DOCTORS (All)"

echo -e "\n${GREEN}GET /doctors${NC}"
curl -s -X GET "$BASE_URL/doctors" \
    -H "Authorization: Bearer $PATIENT_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /doctors/{id}${NC}"
curl -s -X GET "$BASE_URL/doctors/f066397f-db0d-443f-a7f7-eed9751b67f0" \
    -H "Authorization: Bearer $PATIENT_TOKEN" | python3 -m json.tool

# Test Patients endpoints
print_section "5. PATIENTS (Doctor/Clerk)"

echo -e "\n${GREEN}GET /patients (as Doctor)${NC}"
curl -s -X GET "$BASE_URL/patients" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /patients?search=Jean${NC}"
curl -s -X GET "$BASE_URL/patients?search=Jean" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /patients/{id}${NC}"
curl -s -X GET "$BASE_URL/patients/af57425d-b237-4507-bc63-36f8d4e8d5b4" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /patients/{id}/medical-history${NC}"
curl -s -X GET "$BASE_URL/patients/af57425d-b237-4507-bc63-36f8d4e8d5b4/medical-history" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /patients/{id}/notes${NC}"
curl -s -X GET "$BASE_URL/patients/af57425d-b237-4507-bc63-36f8d4e8d5b4/notes" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

# Test User Profile endpoints
print_section "6. USER PROFILE"

echo -e "\n${GREEN}GET /users/me (as Patient)${NC}"
curl -s -X GET "$BASE_URL/users/me" \
    -H "Authorization: Bearer $PATIENT_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /users/me (as Doctor)${NC}"
curl -s -X GET "$BASE_URL/users/me" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

# Test Doctor Dashboard endpoints
print_section "7. DOCTOR DASHBOARD"

echo -e "\n${GREEN}GET /doctors/dashboard${NC}"
curl -s -X GET "$BASE_URL/doctors/dashboard" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /doctors/patients${NC}"
curl -s -X GET "$BASE_URL/doctors/patients" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /doctors/agenda${NC}"
curl -s -X GET "$BASE_URL/doctors/agenda" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" | python3 -m json.tool

# Test Clerk Dashboard endpoints
print_section "8. CLERK DASHBOARD"

echo -e "\n${GREEN}GET /clerk/dashboard${NC}"
curl -s -X GET "$BASE_URL/clerk/dashboard" \
    -H "Authorization: Bearer $CLERK_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /clerk/dashboard/today${NC}"
curl -s -X GET "$BASE_URL/clerk/dashboard/today" \
    -H "Authorization: Bearer $CLERK_TOKEN" | python3 -m json.tool

echo -e "\n${GREEN}GET /clerk/dashboard/pending${NC}"
curl -s -X GET "$BASE_URL/clerk/dashboard/pending" \
    -H "Authorization: Bearer $CLERK_TOKEN" | python3 -m json.tool

# Test Clerk-only endpoints (Create/Delete)
print_section "9. CLERK OPERATIONS"

echo -e "\n${GREEN}POST /patients (create patient - Clerk only)${NC}"
curl -s -X POST "$BASE_URL/patients" \
    -H "Authorization: Bearer $CLERK_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
      "email": "test.newpatient@clinic.com",
      "password": "Password123!",
      "firstName": "Test",
      "lastName": "Patient",
      "phone": "555-1234",
      "dateOfBirth": "1990-01-01T00:00:00Z",
      "address": "123 Test St",
      "emergencyContactName": "Test Contact",
      "emergencyContactPhone": "555-5678"
    }' | python3 -m json.tool

echo -e "\n${GREEN}POST /doctors (create doctor - Clerk only)${NC}"
curl -s -X POST "$BASE_URL/doctors" \
    -H "Authorization: Bearer $CLERK_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
      "email": "test.newdoctor@clinic.com",
      "password": "Password123!",
      "firstName": "Test",
      "lastName": "Doctor",
      "specialization": "Cardiology",
      "licenseNumber": "LIC999",
      "yearsOfExperience": 10
    }' | python3 -m json.tool

echo -e "\n${GREEN}POST /appointments (create appointment)${NC}"
curl -s -X POST "$BASE_URL/appointments" \
    -H "Authorization: Bearer $PATIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
      "patientId": "af57425d-b237-4507-bc63-36f8d4e8d5b4",
      "doctorId": "f066397f-db0d-443f-a7f7-eed9751b67f0",
      "appointmentDate": "2026-01-15T10:00:00Z",
      "reason": "Regular checkup"
    }' | python3 -m json.tool

# Test PUT endpoints
print_section "10. UPDATE OPERATIONS"

echo -e "\n${GREEN}PUT /patients/{id}/notes${NC}"
curl -s -X PUT "$BASE_URL/patients/af57425d-b237-4507-bc63-36f8d4e8d5b4/notes" \
    -H "Authorization: Bearer $DOCTOR_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"notes": "Patient is responding well to treatment."}' | python3 -m json.tool

echo -e "\n${GREEN}PUT /users/me${NC}"
curl -s -X PUT "$BASE_URL/users/me" \
    -H "Authorization: Bearer $PATIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"firstName": "Jean Updated", "phone": "555-9999"}' | python3 -m json.tool

echo -e "\n${GREEN}PUT /users/me/password${NC}"
curl -s -X PUT "$BASE_URL/users/me/password" \
    -H "Authorization: Bearer $PATIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"currentPassword":"Password123!","newPassword":"NewPass123!","confirmPassword":"NewPass123!"}' | python3 -m json.tool

# Test DELETE endpoints
print_section "11. DELETE OPERATIONS"

echo -e "\n${GREEN}DELETE /appointments/{id}${NC}"
APPOINTMENT_ID=$(curl -s -X GET "$BASE_URL/appointments" \
    -H "Authorization: Bearer $CLERK_TOKEN" | python3 -c "import sys, json; data=json.load(sys.stdin); print(data['data'][0]['id'] if data.get('data') and len(data['data']) > 0 else '')")

if [ -n "$APPOINTMENT_ID" ]; then
    echo "Deleting appointment: $APPOINTMENT_ID"
    curl -s -X DELETE "$BASE_URL/appointments/$APPOINTMENT_ID" \
        -H "Authorization: Bearer $PATIENT_TOKEN" | python3 -m json.tool
else
    echo "No appointments to delete"
fi

# Test Authorization errors
print_section "12. AUTHORIZATION TESTS (Should Fail)"

echo -e "\n${RED}Patient trying to access /patients (should be Forbidden)${NC}"
curl -s -X GET "$BASE_URL/patients" \
    -H "Authorization: Bearer $PATIENT_TOKEN" | python3 -m json.tool

echo -e "\n${RED}Patient trying to POST /doctors (should be Forbidden)${NC}"
curl -s -X POST "$BASE_URL/doctors" \
    -H "Authorization: Bearer $PATIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"email":"test@test.com","password":"Pass123!","firstName":"Test","lastName":"Test","specialization":"Test"}' | python3 -m json.tool

echo -e "\n${RED}No token (should be Unauthorized)${NC}"
curl -s -X GET "$BASE_URL/patients" | python3 -m json.tool

print_section "DONE!"
echo -e "\n${GREEN}All endpoint tests completed!${NC}"
