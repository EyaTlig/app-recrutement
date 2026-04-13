using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recrutementapp.Services;
using recrutementapp.ViewModels;

namespace recrutementapp.Controllers;

[Authorize(Policy = "RecruiterAccess")]
public class RecruiterController : Controller
{
    private readonly IJobService         _jobs;
    private readonly IApplicationService _apps;

    public RecruiterController(IJobService jobs, IApplicationService apps)
    {
        _jobs = jobs;
        _apps = apps;
    }

    // GET /Recruiter
    public async Task<IActionResult> Index()
    {
        var uid    = GetUserId();
        var myJobs = await _jobs.GetJobsByUserAsync(uid);
        var myApps = await _apps.GetApplicationsByRecruiterAsync(uid);

        var vm = new RecruiterDashboardViewModel
        {
            TotalJobOffers      = myJobs.Count,
            TotalApplications   = myApps.Count,
            PendingApplications = myApps.Count(a => a.Status == "Pending"),
            InterviewsScheduled = myApps.Count(a => a.Status == "InterviewScheduled"),
            RecentApplications  = myApps.Take(6).ToList(),
            MyJobs              = myJobs.Take(5).ToList()
        };

        return View(vm);
    }

    // GET /Recruiter/Applications
    public async Task<IActionResult> Applications()
    {
        var list = await _apps.GetApplicationsByRecruiterAsync(GetUserId());
        return View(list);
    }

    // GET /Recruiter/ApplicationDetail/5
    public async Task<IActionResult> ApplicationDetail(int id)
    {
        var detail = await _apps.GetApplicationDetailAsync(id);
        if (detail == null) return NotFound();
        return View(detail);
    }

    // POST /Recruiter/UpdateStatus
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        await _apps.UpdateStatusAsync(id, status);
        TempData["Success"] = $"Application status updated to {status}.";
        return RedirectToAction(nameof(ApplicationDetail), new { id });
    }

    // GET /Recruiter/ScheduleInterview/5
    public IActionResult ScheduleInterview(int id)
        => View(new ScheduleInterviewViewModel { ApplicationId = id });

    // POST /Recruiter/ScheduleInterview
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleInterview(ScheduleInterviewViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _apps.ScheduleInterviewAsync(model);
        TempData["Success"] = "Interview scheduled and candidate notified!";
        return RedirectToAction(nameof(ApplicationDetail), new { id = model.ApplicationId });
    }

    // GET /Recruiter/JobOffers
    public async Task<IActionResult> JobOffers()
    {
        var list = await _jobs.GetJobsByUserAsync(GetUserId());
        return View(list);
    }

    // POST /Recruiter/CloseJob
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseJob(int id)
    {
        await _jobs.CloseJobAsync(id, GetUserId());
        TempData["Success"] = "Job offer closed.";
        return RedirectToAction(nameof(JobOffers));
    }

    private int GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(v, out var id) ? id : 0;
    }
}
