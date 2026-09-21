using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Storage.Infrastructure.Clients;

public sealed class IdentityClient
{
    private readonly HttpClient _http;
    private readonly ILogger<IdentityClient> _logger;

    public IdentityClient(HttpClient http, ILogger<IdentityClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IdentityUserDto?> GetUserAsync(Guid userId, string authHeader, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
            if (!string.IsNullOrEmpty(authHeader))
                req.Headers.Add("Authorization", authHeader);

            var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Identity GetUser {Id} returned {Status}", userId, res.StatusCode);
                return null;
            }
            return await res.Content.ReadFromJsonAsync<IdentityUserDto>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user {UserId} from Identity", userId);
            return null;
        }
    }

    public async Task<List<IdentityUserDto>> ListUsersAsync(string authHeader, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/users");
            if (!string.IsNullOrEmpty(authHeader))
                req.Headers.Add("Authorization", authHeader);

            var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Identity ListUsers returned {Status}", res.StatusCode);
                return new();
            }
            return await res.Content.ReadFromJsonAsync<List<IdentityUserDto>>(cancellationToken: ct) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list users from Identity");
            return new();
        }
    }

    public async Task<bool> SetBlockedAsync(Guid userId, bool blocked, string authHeader, CancellationToken ct = default)
    {
        try
        {
            var action = blocked ? "block" : "unblock";
            var req = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{userId}/{action}");
            if (!string.IsNullOrEmpty(authHeader))
                req.Headers.Add("Authorization", authHeader);

            var res = await _http.SendAsync(req, ct);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set blocked {UserId}", userId);
            return false;
        }
    }
}

public sealed record IdentityUserDto(
    Guid Id, string Email, string FirstName, string LastName,
    string FullName, string? PhoneNumber, bool IsActive, bool IsSeller,
    DateTime CreatedAt, List<string> Roles);
