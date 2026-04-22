namespace MauiDexChallenge.Shared.Models;

public sealed record ParsedDexReport(
    DexMeterRecord Meter,
    IReadOnlyCollection<DexLaneMeterRecord> LaneMeters);
