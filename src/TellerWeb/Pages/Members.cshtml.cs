using TellerWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TellerWeb.Pages;

public class MembersModel : PageModel
{
    private readonly TellerDb _db;

    public MembersModel(TellerDb db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public List<Member> Results { get; private set; } = new();
    public string? Error { get; private set; }

    public void OnGet()
    {
        try
        {
            Results = _db.SearchMembers(Q, 60);
        }
        catch (Exception ex)
        {
            Error = "Member database unavailable: " + ex.Message;
        }
    }
}
