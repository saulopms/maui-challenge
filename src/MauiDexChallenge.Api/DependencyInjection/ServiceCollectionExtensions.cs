using MauiDexChallenge.Api.Authentication;
using MauiDexChallenge.Api.Persistence;
using MauiDexChallenge.Infra.Persistence;
using MauiDexChallenge.Shared.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace MauiDexChallenge.Api.DependencyInjection
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMauiDexApiServices(this IServiceCollection services)
        {
            services.AddSingleton<BasicAuthValidator>();
            services.AddSingleton<IDexParser, DexParser>();
            services.AddScoped<IDexRepository, SqlDexRepository>();
            services.AddSingleton<DatabaseScriptExecutor>();
            services.AddHostedService<DatabaseInitializer>();

            return services;
        }
    }
}
