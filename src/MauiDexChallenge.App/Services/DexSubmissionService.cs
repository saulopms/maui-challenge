using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using MauiDexChallenge.Shared.Contracts;
using MauiDexChallenge.Shared.Enums;

namespace MauiDexChallenge.App.Services;

public sealed class DexSubmissionService : IDexSubmissionService
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;

    public DexSubmissionService(HttpClient httpClient, AppSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task ClearDatabaseAsync(string baseUrl, CancellationToken cancellationToken)
    {
        string sanitizedBaseUrl = baseUrl.Trim().TrimEnd('/');
        string requestUri = $"{sanitizedBaseUrl}/admin/clear-tables";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.Username}:{_settings.Password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? $"The API returned status code {(int)response.StatusCode}."
                : error);
        }
    }

    public async Task<DexSubmissionResult> SubmitAsync(string baseUrl, MachineType machine, CancellationToken cancellationToken)
    {
        string sanitizedBaseUrl = baseUrl.Trim().TrimEnd('/');
        string requestUri = $"{sanitizedBaseUrl}/vdi-dex?machine={machine.ToApiValue()}";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(DexReportCatalog.GetReport(machine), Encoding.UTF8, "text/plain")
        };

        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.Username}:{_settings.Password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            DexSubmissionResult? payload = await response.Content.ReadFromJsonAsync<DexSubmissionResult>(cancellationToken: cancellationToken);
            if (payload is not null)
            {
                return payload;
            }
        }

        string error = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
            ? $"The API returned status code {(int)response.StatusCode}."
            : error);
    }
}
