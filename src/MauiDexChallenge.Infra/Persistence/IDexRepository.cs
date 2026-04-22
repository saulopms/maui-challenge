using MauiDexChallenge.Shared.Models;

namespace MauiDexChallenge.Infra.Persistence;

public interface IDexRepository
{
    Task SaveAsync(ParsedDexReport report, CancellationToken cancellationToken);
    Task ClearAsync(CancellationToken cancellationToken);
}
