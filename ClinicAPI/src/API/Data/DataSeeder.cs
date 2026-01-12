using API.Data.Factories;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

/// <summary>
/// Database seeder using factory pattern (similar to Laravel's seeders)
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Seed the database with diverse test data
    /// </summary>
    public static async Task SeedDataAsync(ClinicDbContext context)
    {
        // Only seed if database is empty
        if (await context.Users.AnyAsync())
        {
            return;
        }

        Console.WriteLine("Seeding database...");

        // Create factories
        var faker = new Bogus.Faker("fr");
        Console.WriteLine("Created faker");

        var userFactory = new UserFactory();
        var patientFactory = new PatientFactory();
        var doctorFactory = new DoctorFactory();
        var clerkFactory = new ClerkFactory();
        var appointmentFactory = new AppointmentFactory();
        Console.WriteLine("Created factories");

        #region Seed Users and Role-Specific Data

        // ============ PATIENTS ============
        Console.WriteLine("Creating patients...");
        var patientUsers = new List<User>();
        var patients = new List<Patient>();

        // Generate test patient account (following same pattern as seeded accounts)
        var testPatient = new UserFactory(fixedRole: UserRole.Patient).GenerateWithEmail("patient.jean.dupont@clinic.com");
        testPatient.FirstName = "Jean";
        testPatient.LastName = "Dupont";
        patientUsers.Add(testPatient);

        // Generate 7 random patients
        for (int i = 0; i < 7; i++)
        {
            var user = new UserFactory(fixedRole: UserRole.Patient).Generate();
            user.Email = user.Email.ToLower();
            patientUsers.Add(user);
        }
        Console.WriteLine($"Generated {patientUsers.Count} patient users");

        // Create patient profiles
        foreach (var patientUser in patientUsers)
        {
            patients.Add(patientFactory.GenerateForUser(patientUser.Id));
        }
        Console.WriteLine($"Created {patients.Count} patient profiles");

        // ============ DOCTORS ============
        Console.WriteLine("Creating doctors...");
        var doctorUsers = new List<User>();
        var doctors = new List<Doctor>();

        // Generate test doctor account (following same pattern as seeded accounts)
        var testDoctor = new UserFactory(fixedRole: UserRole.Doctor).GenerateWithEmail("doctor.martin.dupont@clinic.com");
        testDoctor.FirstName = "Martin";
        testDoctor.LastName = "Dupont";
        doctorUsers.Add(testDoctor);

        // Generate 3 random doctors
        for (int i = 0; i < 3; i++)
        {
            var user = new UserFactory(fixedRole: UserRole.Doctor).Generate();
            user.Email = user.Email.ToLower();
            doctorUsers.Add(user);
        }
        Console.WriteLine($"Generated {doctorUsers.Count} doctor users");

        // Create doctor profiles
        foreach (var doctorUser in doctorUsers)
        {
            doctors.Add(doctorFactory.GenerateForUser(doctorUser.Id));
        }
        Console.WriteLine($"Created {doctors.Count} doctor profiles");

        // ============ CLERK ============
        Console.WriteLine("Creating clerks...");
        var clerkUsers = new List<User>();
        var clerks = new List<Clerk>();

        // Generate test clerk account (following same pattern as seeded accounts)
        var testClerk = new UserFactory(fixedRole: UserRole.Clerk).GenerateWithEmail("clerk.claire.laurent@clinic.com");
        testClerk.FirstName = "Claire";
        testClerk.LastName = "Laurent";
        clerkUsers.Add(testClerk);

        // Create clerk profile
        foreach (var clerkUser in clerkUsers)
        {
            clerks.Add(clerkFactory.GenerateForUser(clerkUser.Id));
        }
        Console.WriteLine($"Created {clerks.Count} clerk profiles");

        #endregion

        #region Save Users and Role Data

        Console.WriteLine("Saving users and role data...");
        await context.Users.AddRangeAsync(patientUsers);
        await context.Users.AddRangeAsync(doctorUsers);
        await context.Users.AddRangeAsync(clerkUsers);

        await context.Patients.AddRangeAsync(patients);
        await context.Doctors.AddRangeAsync(doctors);
        await context.Clerks.AddRangeAsync(clerks);

        Console.WriteLine("Calling SaveChangesAsync...");
        await context.SaveChangesAsync();

        Console.WriteLine($"✓ Seeded {patientUsers.Count} patients");
        Console.WriteLine($"✓ Seeded {doctorUsers.Count} doctors");
        Console.WriteLine($"✓ Seeded {clerkUsers.Count} clerks");

        #endregion

        #region Seed Appointments

        Console.WriteLine("Creating appointments...");
        var appointments = new List<Appointment>();
        var doctorIds = doctors.Select(d => d.Id).ToList();
        var clerkId = clerkUsers.First().Id;

        // Generate 2-4 appointments per patient
        foreach (var patient in patients)
        {
            var createdBy = faker.PickRandom(doctorIds.Concat(new[] { clerkId }).ToList());
            var patientAppointments = appointmentFactory.GenerateForPatient(
                patient.Id,
                doctorIds,
                createdBy,
                faker.Random.Int(2, 4)
            ).ToList();

            appointments.AddRange(patientAppointments);
        }
        Console.WriteLine($"Generated {appointments.Count} appointments");

        Console.WriteLine("Saving appointments...");
        await context.Appointments.AddRangeAsync(appointments);
        await context.SaveChangesAsync();

        var scheduled = appointments.Count(a => a.Status == AppointmentStatus.Scheduled);
        var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var noShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow);

        Console.WriteLine($"✓ Seeded {appointments.Count} appointments:");
        Console.WriteLine($"  - {scheduled} scheduled");
        Console.WriteLine($"  - {completed} completed");
        Console.WriteLine($"  - {cancelled} cancelled");
        Console.WriteLine($"  - {noShow} no-show");

        #endregion

        Console.WriteLine("Database seeding completed!");
    }
}
