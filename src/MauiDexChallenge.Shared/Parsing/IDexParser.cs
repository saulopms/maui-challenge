using MauiDexChallenge.Shared.Enums;
using MauiDexChallenge.Shared.Models;

namespace MauiDexChallenge.Shared.Parsing;

public interface IDexParser
{
    ParsedDexReport Parse(string rawDexContent, MachineType machine);
}
