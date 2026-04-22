using System.Data;
using MauiDexChallenge.Shared.Models;
using Microsoft.Data.SqlClient;

namespace MauiDexChallenge.Api.Persistence;

public sealed class SqlDexRepository : IDexRepository
{
    private readonly IConfiguration _configuration;

    public SqlDexRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SaveAsync(ParsedDexReport report, CancellationToken cancellationToken)
    {
        string? connectionString = _configuration.GetConnectionString("DexDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DexDatabase' was not configured.");
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            Guid dexMeterId = await SaveDexMeterAsync(connection, transaction, report.Meter, cancellationToken);

            foreach (DexLaneMeterRecord laneMeter in report.LaneMeters)
            {
                await SaveDexLaneMeterAsync(connection, transaction, dexMeterId, laneMeter, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<Guid> SaveDexMeterAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        DexMeterRecord meter,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("dbo.SaveDexMeter", connection, transaction)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@Machine", meter.Machine);
        command.Parameters.AddWithValue("@DexDateTime", meter.DexDateTime);
        command.Parameters.AddWithValue("@MachineSerialNumber", meter.MachineSerialNumber);
        command.Parameters.AddWithValue("@ValueOfPaidVends", meter.ValueOfPaidVends);
        command.Parameters.AddWithValue("@RawDexContent", meter.RawDexContent);

        var outputParameter = new SqlParameter("@DexMeterId", SqlDbType.UniqueIdentifier)
        {
            Direction = ParameterDirection.Output
        };

        command.Parameters.Add(outputParameter);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return (Guid)outputParameter.Value;
    }

    private static async Task SaveDexLaneMeterAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid dexMeterId,
        DexLaneMeterRecord laneMeter,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("dbo.SaveDexLaneMeter", connection, transaction)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@DexMeterId", dexMeterId);
        command.Parameters.AddWithValue("@ProductIdentifier", laneMeter.ProductIdentifier);
        command.Parameters.AddWithValue("@Price", laneMeter.Price);
        command.Parameters.AddWithValue("@NumberOfVends", laneMeter.NumberOfVends);
        command.Parameters.AddWithValue("@ValueOfPaidSales", laneMeter.ValueOfPaidSales);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
