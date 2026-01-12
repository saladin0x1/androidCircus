using API.Models;
using BCrypt.Net;

namespace API.Data.Factories;

/// <summary>
/// Factory for generating User test data
/// </summary>
public class UserFactory : Factory<User>
{
    private readonly string _defaultPassword;
    private readonly UserRole? _fixedRole;

    public UserFactory(string defaultPassword = "Password123!", UserRole? fixedRole = null)
    {
        _defaultPassword = defaultPassword;
        _fixedRole = fixedRole;
    }

    public override User Generate()
    {
        var role = _fixedRole ?? Faker.PickRandom<UserRole>();
        var firstName = Faker.Name.FirstName();
        var lastName = Faker.Name.LastName();

        // Generate role-prefixed email for easy identification
        var rolePrefix = role switch
        {
            UserRole.Patient => "patient",
            UserRole.Doctor => "doctor",
            UserRole.Clerk => "clerk",
            _ => "user"
        };

        // Format: role.firstname.lastname@clinic.com
        var email = $"{rolePrefix}.{firstName.ToLower()}.{lastName.ToLower()}@clinic.com";

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(_defaultPassword),
            FirstName = firstName,
            LastName = lastName,
            Phone = Faker.Phone.PhoneNumber("+# #########"),
            Role = role,
            IsActive = true,
            CreatedAt = Faker.Date.Past(refDate: DateTime.UtcNow)
        };
    }

    /// <summary>
    /// Generate a patient user
    /// </summary>
    public User GeneratePatient()
    {
        return Generate(user => user.Role = UserRole.Patient);
    }

    /// <summary>
    /// Generate a doctor user
    /// </summary>
    public User GenerateDoctor()
    {
        return Generate(user => user.Role = UserRole.Doctor);
    }

    /// <summary>
    /// Generate a clerk user
    /// </summary>
    public User GenerateClerk()
    {
        return Generate(user => user.Role = UserRole.Clerk);
    }

    /// <summary>
    /// Generate with custom email (useful for test accounts)
    /// </summary>
    public User GenerateWithEmail(string email)
    {
        return Generate(user => user.Email = email);
    }
}
