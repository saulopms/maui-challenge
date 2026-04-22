using System.Data;
using System.Text.RegularExpressions;
using MauiDexChallenge.Api.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace MauiDexChallenge.Api.Persistence;

public sealed class DatabaseInitializer : IHostedService
{
    private static readonly Regex BatchSplitter = new(
        @"^\s*GO\s*$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IConfiguration _configuration;
    private readonly DatabaseOptions _databaseOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IConfiguration configuration,
        IOptions<DatabaseOptions> databaseOptions,
        IWebHostEnvironment environment,
        ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _databaseOptions = databaseOptions.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_databaseOptions.InitializeOnStartup)
        {
            _logger.LogInformation("Database initialization is disabled.");
            return;
        }

        string? connectionString = _configuration.GetConnectionString("DexDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DexDatabase' was not configured.");
        }

        string sqlDirectory = Path.Combine(_environment.ContentRootPath, "Sql");
        string createDatabaseScriptPath = Path.Combine(sqlDirectory, "001-create-database.sql");
        string schemaScriptPath = Path.Combine(sqlDirectory, "002-schema.sql");

        await ExecuteScriptAsync(BuildMasterConnectionString(connectionString), createDatabaseScriptPath, cancellationToken);
        await ExecuteScriptAsync(connectionString, schemaScriptPath, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ExecuteScriptAsync(string connectionString, string scriptPath, CancellationToken cancellationToken)
    {
        string script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
        string[] batches = BatchSplitter.Split(script);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (string batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = batch;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string BuildMasterConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        return builder.ConnectionString;
    }
}
