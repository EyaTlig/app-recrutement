using Microsoft.AspNetCore.Mvc;
using recrutementapp.Services;

namespace recrutementapp.Controllers;

public class HomeController : Controller
{
    private readonly IJobService _jobs;
    public HomeController(IJobService jobs) => _jobs = jobs;

    public async Task<IActionResult> Index()
    {
        var latest = await _jobs.GetActiveJobsAsync(null, null, null, 1, 6);
        return View(latest);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
