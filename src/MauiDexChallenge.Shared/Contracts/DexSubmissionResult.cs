namespace MauiDexChallenge.Shared.Contracts;

public sealed record DexSubmissionResult(
    bool Success,
    string Machine,
    DateTime DexDateTime,
    int LaneCount,
    string Message);
