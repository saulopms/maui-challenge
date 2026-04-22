namespace MauiDexChallenge.Infra.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool InitializeOnStartup { get; init; }
}
