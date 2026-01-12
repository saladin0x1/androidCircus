using Bogus;

namespace API.Data.Factories;

/// <summary>
/// Base factory class for generating test data. Similar to Laravel's Factory pattern.
/// </summary>
public abstract class Factory<T> where T : class
{
    protected readonly Faker Faker;

    protected Factory()
    {
        Faker = new Faker("en"); // Using English locale temporarily to test
    }

    /// <summary>
    /// Generate a single entity
    /// </summary>
    public abstract T Generate();

    /// <summary>
    /// Generate multiple entities
    /// </summary>
    public IEnumerable<T> Generate(int count)
    {
        return Enumerable.Range(0, count).Select(_ => Generate());
    }

    /// <summary>
    /// Generate with custom overrides
    /// </summary>
    public T Generate(Action<T> overrides)
    {
        var entity = Generate();
        overrides(entity);
        return entity;
    }

    /// <summary>
    /// Generate multiple entities with custom overrides for each
    /// </summary>
    public IEnumerable<T> Generate(int count, Func<int, T>? overrides = null)
    {
        return Enumerable.Range(0, count).Select(i =>
        {
            var entity = Generate();
            if (overrides != null)
            {
                var overridden = overrides(i);
                return overridden;
            }
            return entity;
        });
    }
}
