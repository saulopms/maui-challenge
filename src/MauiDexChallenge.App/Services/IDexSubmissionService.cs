using MauiDexChallenge.Shared.Contracts;
using MauiDexChallenge.Shared.Enums;

namespace MauiDexChallenge.App.Services;

public interface IDexSubmissionService
{
    Task<DexSubmissionResult> SubmitAsync(string baseUrl, MachineType machine, CancellationToken cancellationToken);
    Task ClearDatabaseAsync(string baseUrl, CancellationToken cancellationToken);
}
