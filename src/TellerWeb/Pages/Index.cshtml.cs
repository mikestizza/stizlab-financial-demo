using TellerWeb.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TellerWeb.Pages;

public class IndexModel : PageModel
{
    private readonly TellerDb _db;
    private readonly LedgerClient _ledger;
    private readonly ILogger<IndexModel> _log;

    public IndexModel(TellerDb db, LedgerClient ledger, ILogger<IndexModel> log)
    {
        _db = db;
        _ledger = ledger;
        _log = log;
    }

    public int Members { get; private set; }
    public int LookupsToday { get; private set; }
    public int JobsToday { get; private set; }
    public bool DbOk { get; private set; }
    public bool LedgerOk { get; private set; }
    public List<LookupEntry> Recent { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        DbOk = await _db.PingAsync(ct);
        LedgerOk = await _ledger.PingAsync(ct);

        if (DbOk)
        {
            try
            {
                (Members, LookupsToday, JobsToday) = _db.Stats();
                Recent = _db.RecentLookups(8);
            }
            catch (Exception ex)
            {
                // Schema may still be initializing on first boot.
                _log.LogWarning("Dashboard query failed: {Message}", ex.Message);
                DbOk = false;
            }
        }
    }
}
