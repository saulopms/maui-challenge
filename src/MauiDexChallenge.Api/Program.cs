using MauiDexChallenge.Api.DependencyInjection;
using MauiDexChallenge.Api.Endpoints;
using MauiDexChallenge.Api.Options;
using MauiDexChallenge.Infra.Options;
using MauiDexChallenge.Infra.Persistence;


var builder = WebApplication.CreateBuilder(args);

// Configure options from IConfiguration
builder.Services.Configure<ApiAuthOptions>(builder.Configuration.GetSection(ApiAuthOptions.SectionName));
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));

// Register API services
builder.Services.AddMauiDexApiServices();

var app = builder.Build();

// Map endpoints
app.MapVdiDexEndpoints();

app.Run();
