using TellerWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TellerWeb.Pages;

public class MemberModel : PageModel
{
    private readonly TellerDb _db;
    private readonly LedgerClient _ledger;
    private readonly AppInfo _info;

    public MemberModel(TellerDb db, LedgerClient ledger, AppInfo info)
    {
        _db = db;
        _ledger = ledger;
        _info = info;
    }

    public int Id { get; private set; }
    public Member? Member { get; private set; }
    public LedgerResult? Ledger { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        Id = id;
        Member = _db.GetMember(id);
        if (Member is null) return Page();

        Ledger = await _ledger.GetAccountsAsync(id, ct);
        try { _db.LogLookup(id, _info.Pod); } catch { /* non-critical */ }
        return Page();
    }
}
