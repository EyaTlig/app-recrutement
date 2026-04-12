using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using recrutementapp.Data;
using recrutementapp.Services;
using recrutementapp.ViewModels;

namespace recrutementapp.Controllers;

public class JobController : Controller
{
    private readonly IJobService         _jobs;
    private readonly IApplicationService _apps;
    private readonly ApplicationDbContext _db;
    private const int PageSize = 9;

    public JobController(IJobService jobs, IApplicationService apps, ApplicationDbContext db)
    {
        _jobs = jobs;
        _apps = apps;
        _db   = db;
    }

    // GET /Job
    public async Task<IActionResult> Index(string? search, string? location, string? contractType, int page = 1)
    {
        var jobs  = await _jobs.GetActiveJobsAsync(search, location, contractType, page, PageSize);
        var total = await _jobs.GetActiveJobsCountAsync(search, location, contractType);

        // Mark already-applied jobs for logged-in candidates
        if (User.IsInRole("Candidate"))
        {
            var uid = GetUserId();
            if (uid.HasValue)
                foreach (var j in jobs)
                    j.HasApplied = await _apps.HasAppliedAsync(uid.Value, j.Id);
        }

        ViewBag.Search       = search;
        ViewBag.Location     = location;
        ViewBag.ContractType = contractType;
        ViewBag.CurrentPage  = page;
        ViewBag.TotalPages   = (int)Math.Ceiling((double)total / PageSize);

        return View(jobs);
    }

    // GET /Job/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var job = await _jobs.GetJobDetailAsync(id, GetUserId());
        if (job == null) return NotFound();
        return View(job);
    }

    // GET /Job/Create  — Recruiter / Admin only
    [Authorize(Policy = "RecruiterAccess")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Skills = await _db.Skills.OrderBy(s => s.Category).ThenBy(s => s.Name).ToListAsync();
        return View(new CreateJobViewModel());
    }

    // POST /Job/Create
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "RecruiterAccess")]
    public async Task<IActionResult> Create(CreateJobViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Skills = await _db.Skills.OrderBy(s => s.Category).ThenBy(s => s.Name).ToListAsync();
            return View(model);
        }

        var uid = GetUserId()!.Value;

        // Get recruiter's company
        var recruiter = await _db.RecruiterProfiles.FirstOrDefaultAsync(r => r.UserId == uid);
        int companyId = recruiter?.CompanyId ?? 1;

        await _jobs.CreateJobAsync(model, companyId, uid);
        TempData["Success"] = "Job offer published successfully!";
        return RedirectToAction("Index", "Recruiter");
    }

    // GET /Job/Apply/5
    [Authorize(Policy = "CandidateOnly")]
    public async Task<IActionResult> Apply(int id)
    {
        var job = await _jobs.GetJobDetailAsync(id, GetUserId());
        if (job == null) return NotFound();

        var uid = GetUserId()!.Value;
        if (await _apps.HasAppliedAsync(uid, id))
        {
            TempData["Info"] = "You have already applied to this position.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(new ApplyViewModel
        {
            JobOfferId  = id,
            JobTitle    = job.Title,
            CompanyName = job.CompanyName
        });
    }

    // POST /Job/Apply
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "CandidateOnly")]
    public async Task<IActionResult> Apply(ApplyViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _apps.SubmitApplicationAsync(model, GetUserId()!.Value);
        TempData["Success"] = "Your application has been submitted successfully!";
        return RedirectToAction("Dashboard", "Candidate");
    }

    private int? GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(v, out var id) ? id : null;
    }
}
