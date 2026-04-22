using System.Globalization;
using MauiDexChallenge.Shared.Enums;
using MauiDexChallenge.Shared.Models;

namespace MauiDexChallenge.Shared.Parsing;

public sealed class DexParser : IDexParser
{
    private const char SegmentSeparator = '*';

    public ParsedDexReport Parse(string rawDexContent, MachineType machine)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawDexContent);

        string? machineSerialNumber = null;
        string? rawDate = null;
        string? rawTime = null;
        string? rawSecond = null;
        decimal? valueOfPaidVends = null;

        var laneMeters = new List<DexLaneMeterRecord>();
        LaneMeterAccumulator? currentLaneMeter = null;

        foreach (string rawLine in SplitLines(rawDexContent))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] fields = line.Split(SegmentSeparator);
            string segment = fields[0];

            switch (segment)
            {
                case "ID1":
                    machineSerialNumber = GetRequiredField(fields, 1, segment, "machine serial number");
                    break;

                case "ID5":
                    rawDate = GetRequiredField(fields, 1, segment, "DEX date");
                    rawTime = GetRequiredField(fields, 2, segment, "DEX time");
                    rawSecond = GetRequiredField(fields, 3, segment, "DEX second");
                    break;

                case "VA1":
                    valueOfPaidVends = ParseDexCurrency(GetRequiredField(fields, 1, segment, "value of paid vends"));
                    break;

                case "PA1":
                    FlushLaneMeter(currentLaneMeter, laneMeters);
                    currentLaneMeter = new LaneMeterAccumulator(
                        ProductIdentifier: GetRequiredField(fields, 1, segment, "product identifier"),
                        Price: ParseDexCurrency(GetRequiredField(fields, 2, segment, "price")));
                    break;

                case "PA2":
                    if (currentLaneMeter is null)
                    {
                        throw new InvalidOperationException("PA2 segment found before PA1 segment.");
                    }

                    currentLaneMeter.NumberOfVends = ParseInt(GetRequiredField(fields, 1, segment, "number of vends"));
                    currentLaneMeter.ValueOfPaidSales = ParseDexCurrency(GetRequiredField(fields, 2, segment, "value of paid sales"));
                    break;
            }
        }

        FlushLaneMeter(currentLaneMeter, laneMeters);

        if (machineSerialNumber is null)
        {
            throw new InvalidOperationException("The DEX report does not contain an ID1 segment.");
        }

        if (rawDate is null || rawTime is null || rawSecond is null)
        {
            throw new InvalidOperationException("The DEX report does not contain a complete ID5 segment.");
        }

        if (valueOfPaidVends is null)
        {
            throw new InvalidOperationException("The DEX report does not contain a VA1 segment.");
        }

        if (laneMeters.Count == 0)
        {
            throw new InvalidOperationException("The DEX report does not contain lane meter data.");
        }

        var meter = new DexMeterRecord(
            machine.ToApiValue(),
            ParseDexDateTime(rawDate, rawTime, rawSecond),
            machineSerialNumber,
            valueOfPaidVends.Value,
            rawDexContent);

        return new ParsedDexReport(meter, laneMeters);
    }

    private static IEnumerable<string> SplitLines(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string GetRequiredField(string[] fields, int index, string segment, string fieldDescription)
    {
        if (fields.Length <= index || string.IsNullOrWhiteSpace(fields[index]))
        {
            throw new InvalidOperationException($"Segment {segment} is missing the {fieldDescription} field.");
        }

        return fields[index];
    }

    private static int ParseInt(string rawValue)
    {
        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            throw new InvalidOperationException($"Unable to parse integer value '{rawValue}'.");
        }

        return value;
    }

    private static decimal ParseDexCurrency(string rawValue)
    {
        if (!decimal.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal cents))
        {
            throw new InvalidOperationException($"Unable to parse currency value '{rawValue}'.");
        }

        return cents / 100m;
    }

    private static DateTime ParseDexDateTime(string rawDate, string rawTime, string rawSecond)
    {
        string normalizedTime = rawTime.PadLeft(4, '0');
        string normalizedSecond = rawSecond.PadLeft(2, '0');
        string combined = $"{rawDate}{normalizedTime}{normalizedSecond}";

        if (!DateTime.TryParseExact(
                combined,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out DateTime parsed))
        {
            throw new InvalidOperationException($"Unable to parse DEX date/time '{combined}'.");
        }

        return parsed;
    }

    private static void FlushLaneMeter(LaneMeterAccumulator? currentLaneMeter, ICollection<DexLaneMeterRecord> laneMeters)
    {
        if (currentLaneMeter is null)
        {
            return;
        }

        laneMeters.Add(new DexLaneMeterRecord(
            currentLaneMeter.ProductIdentifier,
            currentLaneMeter.Price,
            currentLaneMeter.NumberOfVends,
            currentLaneMeter.ValueOfPaidSales));
    }

    private sealed class LaneMeterAccumulator
    {
        public LaneMeterAccumulator(string ProductIdentifier, decimal Price)
        {
            this.ProductIdentifier = ProductIdentifier;
            this.Price = Price;
        }

        public string ProductIdentifier { get; }

        public decimal Price { get; }

        public int NumberOfVends { get; set; }

        public decimal ValueOfPaidSales { get; set; }
    }
}
