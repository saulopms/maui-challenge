using MauiDexChallenge.Api.Authentication;
using MauiDexChallenge.Infra.Persistence;
using MauiDexChallenge.Shared.Contracts;
using MauiDexChallenge.Shared.Enums;
using MauiDexChallenge.Shared.Parsing;
using Microsoft.Data.SqlClient;

namespace MauiDexChallenge.Api.Endpoints
{
    internal static class VdiDexEndpoints
    {
        public static void MapVdiDexEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => Results.Ok(new
            {
                name = "Maui DEX Challenge API",
                status = "running",
                endpoint = "/vdi-dex"
            }));

            app.MapPost("/vdi-dex", async (
                HttpRequest request,
                BasicAuthValidator authValidator,
                IDexParser parser,
                IDexRepository repository,
                ILoggerFactory loggerFactory,
                string machine,
                CancellationToken cancellationToken) =>
            {
                if (!authValidator.IsAuthorized(request.Headers.Authorization))
                {
                    return Results.Unauthorized();
                }

                MachineType machineType;
                try
                {
                    machineType = Enum.Parse<MachineType>(machine, ignoreCase: true);
                }
                catch (ArgumentException)
                {
                    return Results.BadRequest("Machine must be either A or B.");
                }

                using var reader = new StreamReader(request.Body);
                string rawDexContent = await reader.ReadToEndAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(rawDexContent))
                {
                    return Results.BadRequest("Request body must contain a DEX payload.");
                }

                try
                {
                    var report = parser.Parse(rawDexContent, machineType);
                    await repository.SaveAsync(report, cancellationToken);

                    var response = new DexSubmissionResult(
                        true,
                        report.Meter.Machine,
                        report.Meter.DexDateTime,
                        report.LaneMeters.Count,
                        "DEX payload processed successfully.");

                    return Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (SqlException ex) when (ex.Number is 2601 or 2627)
                {
                    return Results.Conflict("A DEX meter entry already exists for this machine and DEX date/time.");
                }
                catch (Exception ex)
                {
                    ILogger logger = loggerFactory.CreateLogger("VdiDexEndpoint");
                    logger.LogError(ex, "Unhandled error while processing a DEX payload.");
                    return Results.Problem("An unexpected error occurred while processing the DEX payload.");
                }
            });
        }
    }
}
