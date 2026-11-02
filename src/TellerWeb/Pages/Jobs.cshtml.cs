using TellerWeb.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TellerWeb.Pages;

public class JobsModel : PageModel
{
    private readonly TellerDb _db;
    public JobsModel(TellerDb db) => _db = db;

    public List<JobRun> Runs { get; private set; } = new();
    public string? Error { get; private set; }

    public void OnGet()
    {
        try { Runs = _db.RecentJobs(30); }
        catch (Exception ex) { Error = "Job history unavailable: " + ex.Message; }
    }
}
