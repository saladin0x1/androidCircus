using API.Models;

namespace API.Data.Factories;

/// <summary>
/// Factory for generating Patient test data
/// </summary>
public class PatientFactory : Factory<Patient>
{
    public override Patient Generate()
    {
        return new Patient
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Empty, // Must be set explicitly
            DateOfBirth = Faker.Date.Past(refDate: DateTime.UtcNow.AddYears(18)),
            Address = Faker.Address.FullAddress(),
            EmergencyContactName = Faker.Name.FullName(),
            EmergencyContactPhone = Faker.Phone.PhoneNumber("+# #########"),
            RegistrationDate = Faker.Date.Past(refDate: DateTime.UtcNow)
        };
    }

    /// <summary>
    /// Generate for a specific user ID
    /// </summary>
    public Patient GenerateForUser(Guid userId)
    {
        return Generate(patient => patient.UserId = userId);
    }
}
