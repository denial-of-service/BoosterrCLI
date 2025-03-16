namespace Boosterr;

public record MediaManagerInstance
{
    public required string Name { get; init; }
    public required MediaManagerType Type { get; init; }
    public required bool IsEnabled { get; init; }
    public required bool ShouldOverwriteNonBoosterrCustomFormats { get; init; }
    public required bool ShouldDeleteNonBoosterrCustomFormats { get; init; }
    public required string Url { get; init; }
    public required string ApiKey { get; init; }
}