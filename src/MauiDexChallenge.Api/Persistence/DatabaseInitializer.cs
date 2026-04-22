using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MauiDexChallenge.Infra.Options;
using MauiDexChallenge.Infra.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MauiDexChallenge.Api.Persistence
{
    internal sealed class DatabaseInitializer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseOptions _databaseOptions;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DatabaseInitializer> _logger;
        private readonly DatabaseScriptExecutor _executor;

        public DatabaseInitializer(
            IConfiguration configuration,
            IOptions<DatabaseOptions> databaseOptions,
            IWebHostEnvironment environment,
            ILogger<DatabaseInitializer> logger,
            DatabaseScriptExecutor executor)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _databaseOptions = databaseOptions?.Value ?? new DatabaseOptions();
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_databaseOptions.InitializeOnStartup)
            {
                _logger.LogInformation("Database initialization is disabled.");
                return;
            }

            string? connectionString = _configuration.GetConnectionString("DexDatabase");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogWarning("Connection string 'DexDatabase' was not configured. Skipping DB initialization.");
                return;
            }

            string sqlDirectory = Path.Combine(_environment.ContentRootPath, "Sql");

            if (!Directory.Exists(sqlDirectory))
            {
                // Fallback to output directory (when scripts are copied to build output)
                string outputSqlDir = Path.Combine(AppContext.BaseDirectory, "Sql");
                if (Directory.Exists(outputSqlDir))
                {
                    sqlDirectory = outputSqlDir;
                    _logger.LogInformation("Using SQL directory from output: {SqlDir}", sqlDirectory);
                }
                else
                {
                    _logger.LogWarning("SQL directories not found. Tried '{ContentSql}' and '{OutputSql}'. Skipping DB initialization.",
                        Path.Combine(_environment.ContentRootPath, "Sql"), outputSqlDir);
                    return;
                }
            }

            try
            {
                await _executor.RunScriptsAsync(connectionString, sqlDirectory, stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Database initialization completed.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Database initialization canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed.");
            }
        }
    }
}
