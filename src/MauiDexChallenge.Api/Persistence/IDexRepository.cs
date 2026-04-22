using MauiDexChallenge.Shared.Models;

namespace MauiDexChallenge.Api.Persistence;

public interface IDexRepository
{
    Task SaveAsync(ParsedDexReport report, CancellationToken cancellationToken);
}
