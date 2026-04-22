using System.Net.Http.Headers;
using System.Text;
using MauiDexChallenge.Api.Options;
using Microsoft.Extensions.Options;

namespace MauiDexChallenge.Api.Authentication;

public sealed class BasicAuthValidator
{
    private readonly ApiAuthOptions _options;

    public BasicAuthValidator(IOptions<ApiAuthOptions> options)
    {
        _options = options.Value;
    }

    public bool IsAuthorized(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return false;
        }

        if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out AuthenticationHeaderValue? headerValue))
        {
            return false;
        }

        if (!"Basic".Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return false;
        }

        try
        {
            string credentialText = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue.Parameter));
            int separatorIndex = credentialText.IndexOf(':');
            if (separatorIndex < 0)
            {
                return false;
            }

            string username = credentialText[..separatorIndex];
            string password = credentialText[(separatorIndex + 1)..];

            return string.Equals(username, _options.Username, StringComparison.Ordinal)
                && string.Equals(password, _options.Password, StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
