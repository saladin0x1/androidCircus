using API.Models;

namespace API.Data.Factories;

/// <summary>
/// Factory for generating Doctor test data
/// </summary>
public class DoctorFactory : Factory<Doctor>
{
    private static readonly string[] Specializations =
    {
        "Médecin Généraliste",
        "Cardiologie",
        "Pédiatrie",
        "Dermatologie",
        "Neurologie",
        "Orthopédie",
        "Ophtalmologie",
        "Gynécologie",
        "Psychiatrie",
        "Urologie"
    };

    public override Doctor Generate()
    {
        return new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Empty, // Must be set explicitly
            Specialization = Faker.PickRandom(Specializations),
            LicenseNumber = $"MD{Faker.Random.Number(10000, 99999)}",
            YearsOfExperience = Faker.Random.Number(1, 30),
            JoinedDate = Faker.Date.Past(refDate: DateTime.UtcNow)
        };
    }

    /// <summary>
    /// Generate for a specific user ID
    /// </summary>
    public Doctor GenerateForUser(Guid userId)
    {
        return Generate(doctor => doctor.UserId = userId);
    }

    /// <summary>
    /// Generate with specific specialization
    /// </summary>
    public Doctor GenerateWithSpecialization(string specialization)
    {
        return Generate(doctor => doctor.Specialization = specialization);
    }
}
