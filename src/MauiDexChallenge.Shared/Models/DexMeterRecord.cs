namespace MauiDexChallenge.Shared.Models;

public sealed record DexMeterRecord(
    string Machine,
    DateTime DexDateTime,
    string MachineSerialNumber,
    decimal ValueOfPaidVends,
    string RawDexContent);
