using System.Text.Json;
using System.Text.Json.Serialization;

namespace TellerWeb.Services;

public record LedgerAccount(string AccountId, string Type, decimal Balance, string OpenedOn, string Status);
public record LedgerAccountsResponse(int MemberId, string ServedBy, string CallerIp, LedgerAccount[] Accounts);

public record LedgerResult(bool Ok, string Target, long ElapsedMs, LedgerAccountsResponse? Data, string? Error);

/// <summary>
/// Calls the in-house "Ledger API" over the cluster network. The base URL is a
/// Kubernetes service name (http://ledger-api), so this traffic never leaves
/// the cluster and never touches the external load balancer.
/// </summary>
public sealed class LedgerClient
{
    private readonly HttpClient _http;
    private readonly ILogger<LedgerClient> _log;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public string BaseUrl => _http.BaseAddress?.ToString().TrimEnd('/') ?? "(unset)";

    public LedgerClient(HttpClient http, ILogger<LedgerClient> log)
    {
        _http = http;
        _log = log;
    }

    public async Task<LedgerResult> GetAccountsAsync(int memberId, CancellationToken ct = default)
    {
        var path = $"/api/members/{memberId}/accounts";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var resp = await _http.GetAsync(path, ct);
            sw.Stop();
            if (!resp.IsSuccessStatusCode)
                return new LedgerResult(false, BaseUrl + path, sw.ElapsedMilliseconds, null, $"HTTP {(int)resp.StatusCode}");

            var data = await resp.Content.ReadFromJsonAsync<LedgerAccountsResponse>(JsonOpts, ct);
            return new LedgerResult(true, BaseUrl + path, sw.ElapsedMilliseconds, data, null);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogWarning("Ledger API call failed: {Message}", ex.Message);
            return new LedgerResult(false, BaseUrl + path, sw.ElapsedMilliseconds, null, ex.GetType().Name + ": " + ex.Message);
        }
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.GetAsync("/healthz", ct);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
