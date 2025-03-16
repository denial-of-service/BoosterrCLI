namespace Boosterr;

public enum MediaManagerType
{
    Radarr,
    Sonarr
}

public static class MediaManagerTypeExtensions
{
    public static MediaManagerType Parse(string s)
    {
        if (Enum.TryParse(s, true, out MediaManagerType mediaManagerType)) return mediaManagerType;

        throw new ArgumentException($"'{nameof(s)}' is not a valid MediaManagerType.");
    }

    public static string ToLowercase(this MediaManagerType mediaManagerType)
    {
        return mediaManagerType.ToString().ToLower();
    }
}