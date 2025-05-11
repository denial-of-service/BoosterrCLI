using System.Text.Json;

namespace BoosterrCLI;

public record ApiInfo(string Current, string[] Deprecated)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static ApiInfo Parse(string json)
    {
        return JsonSerializer.Deserialize<ApiInfo>(json, JsonSerializerOptions) ??
               throw new InvalidOperationException();
    }

    public virtual bool Equals(ApiInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Current.Equals(other.Current) && Deprecated.SequenceEqual(other.Deprecated);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Current, Deprecated);
    }
}