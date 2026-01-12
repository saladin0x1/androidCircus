using API.Models;

namespace API.Data.Factories;

/// <summary>
/// Factory for generating Clerk test data
/// </summary>
public class ClerkFactory : Factory<Clerk>
{
    private static readonly string[] Departments =
    {
        "Reception",
        "Administration",
        "Appointments",
        "Records",
        "Billing"
    };

    public override Clerk Generate()
    {
        return new Clerk
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Empty, // Must be set explicitly
            Department = Faker.PickRandom(Departments),
            HireDate = Faker.Date.Past(refDate: DateTime.UtcNow)
        };
    }

    /// <summary>
    /// Generate for a specific user ID
    /// </summary>
    public Clerk GenerateForUser(Guid userId)
    {
        return Generate(clerk => clerk.UserId = userId);
    }
}
