using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using recrutementapp.Data;
using recrutementapp.Services;
using recrutementapp.ViewModels;

namespace recrutementapp.Controllers;

[Authorize(Policy = "CandidateOnly")]
public class CandidateController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IApplicationService  _apps;
    private readonly IJobService          _jobs;

    public CandidateController(ApplicationDbContext db, IApplicationService apps, IJobService jobs)
    {
        _db   = db;
        _apps = apps;
        _jobs = jobs;
    }

    // GET /Candidate/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var uid  = GetUserId();
        var myApps = await _apps.GetApplicationsByUserAsync(uid);

        var vm = new DashboardViewModel
        {
            TotalApplications    = myApps.Count,
            PendingApplications  = myApps.Count(a => a.Status == "Pending"),
            AcceptedApplications = myApps.Count(a => a.Status == "Accepted"),
            InterviewsScheduled  = myApps.Count(a => a.Status == "InterviewScheduled"),
            RecentApplications   = myApps.Take(5).ToList(),
            RecommendedJobs      = await _jobs.GetActiveJobsAsync(null, null, null, 1, 4)
        };

        return View(vm);
    }

    // GET /Candidate/Applications
    public async Task<IActionResult> Applications()
    {
        var list = await _apps.GetApplicationsByUserAsync(GetUserId());
        return View(list);
    }

    // GET /Candidate/Profile
    public async Task<IActionResult> Profile()
    {
        var uid     = GetUserId();
        var user    = await _db.Users.FindAsync(uid);
        var profile = await _db.CandidateProfiles
            .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.UserId == uid);

        var vm = new CandidateProfileViewModel
        {
            UserId    = uid,
            UserName  = user?.Name ?? string.Empty,
            UserEmail = user?.Email ?? string.Empty,
            Headline  = profile?.Headline,
            Location  = profile?.Location,
            Summary   = profile?.Summary,
            ExperienceYears = profile?.ExperienceYears ?? 0,
            LinkedInUrl = profile?.LinkedInUrl,
            GitHubUrl   = profile?.GitHubUrl,
            Skills = profile?.CandidateSkills.Select(cs => new CandidateSkillViewModel
            {
                SkillId          = cs.SkillId,
                SkillName        = cs.Skill.Name,
                Category         = cs.Skill.Category,
                ProficiencyLevel = cs.ProficiencyLevel
            }).ToList() ?? new()
        };

        ViewBag.AllSkills = await _db.Skills.OrderBy(s => s.Category).ThenBy(s => s.Name).ToListAsync();
        return View(vm);
    }

    // POST /Candidate/Profile
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CandidateProfileViewModel model)
    {
        var uid     = GetUserId();
        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == uid);

        if (profile == null)
        {
            profile = new Models.CandidateProfile { UserId = uid };
            _db.CandidateProfiles.Add(profile);
        }

        profile.Headline        = model.Headline;
        profile.Location        = model.Location;
        profile.Summary         = model.Summary;
        profile.ExperienceYears = model.ExperienceYears;
        profile.LinkedInUrl     = model.LinkedInUrl;
        profile.GitHubUrl       = model.GitHubUrl;

        // Update user name
        var user = await _db.Users.FindAsync(uid);
        if (user != null) user.Name = model.UserName;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Profile updated successfully!";
        return RedirectToAction(nameof(Profile));
    }

    // POST /Candidate/AddSkill
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSkill(int skillId, int proficiency)
    {
        var uid     = GetUserId();
        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == uid);
        if (profile == null) return RedirectToAction(nameof(Profile));

        var exists = await _db.CandidateSkills
            .AnyAsync(cs => cs.CandidateProfileId == profile.Id && cs.SkillId == skillId);

        if (!exists)
        {
            _db.CandidateSkills.Add(new Models.CandidateSkill
            {
                CandidateProfileId = profile.Id,
                SkillId            = skillId,
                ProficiencyLevel   = Math.Clamp(proficiency, 1, 5)
            });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Profile));
    }

    // POST /Candidate/RemoveSkill
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSkill(int skillId)
    {
        var uid     = GetUserId();
        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == uid);
        if (profile == null) return RedirectToAction(nameof(Profile));

        var cs = await _db.CandidateSkills
            .FirstOrDefaultAsync(x => x.CandidateProfileId == profile.Id && x.SkillId == skillId);
        if (cs != null)
        {
            _db.CandidateSkills.Remove(cs);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Profile));
    }

    private int GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(v, out var id) ? id : 0;
    }
}
