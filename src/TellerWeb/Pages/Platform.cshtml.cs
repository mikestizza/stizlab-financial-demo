using TellerWeb.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace TellerWeb.Pages;

public class PlatformModel : PageModel
{
    private readonly TellerDb _db;
    private readonly LedgerClient _ledger;
    private readonly IConfiguration _cfg;

    public PlatformModel(TellerDb db, LedgerClient ledger, IConfiguration cfg)
    {
        _db = db;
        _ledger = ledger;
        _cfg = cfg;
    }

    public string User { get; private set; } = "";
    public string SqlServer { get; private set; } = "";
    public string SqlTarget { get; private set; } = "";
    public string LedgerUrl { get; private set; } = "";

    public void OnGet()
    {
        User = Environment.UserName;
        SqlServer = _db.ServerInfo();
        LedgerUrl = _ledger.BaseUrl;

        try
        {
            var b = new SqlConnectionStringBuilder(TellerDb.ResolveConnectionString(_cfg));
            SqlTarget = $"{b.DataSource} / {b.InitialCatalog} (user {b.UserID})";
        }
        catch { SqlTarget = "(unparseable)"; }
    }
}
