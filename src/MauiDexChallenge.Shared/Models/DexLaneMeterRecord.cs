namespace MauiDexChallenge.Shared.Models;

public sealed record DexLaneMeterRecord(
    string ProductIdentifier,
    decimal Price,
    int NumberOfVends,
    decimal ValueOfPaidSales);
