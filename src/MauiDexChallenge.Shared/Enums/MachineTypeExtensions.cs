namespace MauiDexChallenge.Shared.Enums;

public static class MachineTypeExtensions
{
    public static string ToApiValue(this MachineType machine) => machine switch
    {
        MachineType.A => "A",
        MachineType.B => "B",
        _ => throw new ArgumentOutOfRangeException(nameof(machine), machine, "Unsupported machine.")
    };
}
