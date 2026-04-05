using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using recrutementapp.Data;
using recrutementapp.Services;
using recrutementapp.ViewModels;

namespace recrutementapp.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IApplicationService  _apps;
    private readonly IJobService          _jobs;

    public AdminController(ApplicationDbContext db, IApplicationService apps, IJobService jobs)
    {
        _db   = db;
        _apps = apps;
        _jobs = jobs;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new AdminDashboardViewModel
        {
            TotalUsers          = await _db.Users.CountAsync(),
            TotalJobs           = await _db.JobOffers.CountAsync(),
            TotalApplications   = await _db.Applications.CountAsync(),
            TotalCompanies      = await _db.Companies.CountAsync(),
            PendingApplications = await _db.Applications.CountAsync(a => a.Status == "Pending"),
            ActiveJobs          = await _db.JobOffers.CountAsync(j => j.Status == "Active"),
            RecentApplications  = await _db.Applications
                .Include(a => a.User)
                .Include(a => a.JobOffer).ThenInclude(j => j!.Company)
                .OrderByDescending(a => a.AppliedAt)
                .Take(8)
                .Select(a => new ApplicationListItemViewModel
                {
                    Id             = a.Id,
                    JobTitle       = a.JobOffer!.Title,
                    CompanyName    = a.JobOffer.Company != null ? a.JobOffer.Company.Name : "Independent",
                    Status         = a.Status,
                    MatchScore     = a.MatchScore,
                    AppliedAt      = a.AppliedAt,
                    CandidateName  = a.User!.Name,
                    CandidateEmail = a.User.Email
                })
                .ToListAsync()
        };

        return View(vm);
    }

    public async Task<IActionResult> Users()
    {
        var users = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> JobOffers()
    {
        var jobs = await _db.JobOffers
            .Include(j => j.Company)
            .Include(j => j.Applications)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
        return View(jobs);
    }

    public async Task<IActionResult> Applications()
    {
        var apps = await _db.Applications
            .Include(a => a.User)
            .Include(a => a.JobOffer).ThenInclude(j => j!.Company)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new ApplicationListItemViewModel
            {
                Id             = a.Id,
                JobTitle       = a.JobOffer!.Title,
                CompanyName    = a.JobOffer.Company != null ? a.JobOffer.Company.Name : "Independent",
                Status         = a.Status,
                MatchScore     = a.MatchScore,
                AppliedAt      = a.AppliedAt,
                CandidateName  = a.User!.Name,
                CandidateEmail = a.User.Email,
                ResumePath     = a.ResumePath
            })
            .ToListAsync();
        return View(apps);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
        }
        return RedirectToAction(nameof(Users));
    }

    public IActionResult Companies() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _db.JobOffers.FindAsync(id);
        if (job != null)
        {
            job.Status = "Closed";
            await _db.SaveChangesAsync();
            TempData["Success"] = "Job offer closed successfully.";
        }
        return RedirectToAction(nameof(JobOffers));
    }
}
