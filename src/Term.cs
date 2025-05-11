namespace BoosterrCLI;

public record Term(
    bool Sync,
    string Name,
    string PrettyName,
    string Regex,
    string[] TestsMustMatch,
    string[] TestsMustNotMatch);