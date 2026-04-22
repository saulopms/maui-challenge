using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace MauiDexChallenge.Infra.Persistence;

public sealed class DatabaseScriptExecutor
{
    private static readonly Regex BatchSplitter = new(
        @"^\s*GO\s*$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public DatabaseScriptExecutor()
    {
    }

    public async Task RunScriptsAsync(string connectionString, string sqlDirectory, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("connectionString must be provided", nameof(connectionString));
        }
        string createDatabaseScriptPath = Path.Combine(sqlDirectory, "001-create-database.sql");
        string schemaScriptPath = Path.Combine(sqlDirectory, "002-schema.sql");

        if (!File.Exists(createDatabaseScriptPath) || !File.Exists(schemaScriptPath))
        {
            throw new FileNotFoundException($"SQL scripts not found in '{sqlDirectory}'");
        }

        await ExecuteScriptAsync(BuildMasterConnectionString(connectionString), createDatabaseScriptPath, cancellationToken).ConfigureAwait(false);
        await ExecuteScriptAsync(connectionString, schemaScriptPath, cancellationToken).ConfigureAwait(false);
    }

    private static string BuildMasterConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        return builder.ConnectionString;
    }

    private static async Task ExecuteScriptAsync(string connectionString, string scriptPath, CancellationToken cancellationToken)
    {
        string script = await File.ReadAllTextAsync(scriptPath, cancellationToken).ConfigureAwait(false);
        string[] batches = BatchSplitter.Split(script);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        foreach (string batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = batch;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
