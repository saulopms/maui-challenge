namespace MauiDexChallenge.Api.Options;

public sealed class ApiAuthOptions
{
    public const string SectionName = "ApiAuth";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
