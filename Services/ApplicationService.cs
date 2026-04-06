using Microsoft.EntityFrameworkCore;
using recrutementapp.Data;
using recrutementapp.Models;
using recrutementapp.ViewModels;

namespace recrutementapp.Services;

public class ApplicationService : IApplicationService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService  _storage;
    private readonly IEmailSender         _email;
    private readonly INotificationService _notifications;

    public ApplicationService(ApplicationDbContext db, IFileStorageService storage,
        IEmailSender email, INotificationService notifications)
    {
        _db            = db;
        _storage       = storage;
        _email         = email;
        _notifications = notifications;
    }

    public async Task<bool> HasAppliedAsync(int userId, int jobOfferId)
        => await _db.Applications.AnyAsync(a => a.UserId == userId && a.JobOfferId == jobOfferId);

    public async Task<int> SubmitApplicationAsync(ApplyViewModel model, int userId)
    {
        string? resumePath = null;
        if (model.ResumeFile != null)
            resumePath = await _storage.SaveFileAsync(model.ResumeFile, "resumes");

        // Compute simple match score
        var candidateSkillIds = await _db.CandidateSkills
            .Where(cs => cs.CandidateProfile.UserId == userId)
            .Select(cs => cs.SkillId)
            .ToListAsync();

        var jobSkills = await _db.JobSkills
            .Where(js => js.JobOfferId == model.JobOfferId)
            .ToListAsync();

        int matchScore = 50;
        if (jobSkills.Any())
        {
            int req   = jobSkills.Count(js => js.IsRequired);
            int pref  = jobSkills.Count(js => !js.IsRequired);
            int reqM  = jobSkills.Count(js => js.IsRequired  && candidateSkillIds.Contains(js.SkillId));
            int prefM = jobSkills.Count(js => !js.IsRequired && candidateSkillIds.Contains(js.SkillId));
            double s  = (req  > 0 ? (double)reqM  / req  * 70 : 70)
                      + (pref > 0 ? (double)prefM / pref * 30 : 30);
            matchScore = (int)Math.Round(s);
        }

        var app = new Application
        {
            JobOfferId   = model.JobOfferId,
            UserId       = userId,
            CoverLetter  = model.CoverLetter,
            ResumePath   = resumePath,
            Status       = "Pending",
            MatchScore   = matchScore,
            AppliedAt    = DateTime.UtcNow
        };

        _db.Applications.Add(app);
        await _db.SaveChangesAsync();

        // Notify recruiter
        var job = await _db.JobOffers.Include(j => j.Company).FirstOrDefaultAsync(j => j.Id == model.JobOfferId);
        if (job?.PostedByUserId != null)
        {
            var candidate = await _db.Users.FindAsync(userId);
            await _notifications.CreateAsync(
                job.PostedByUserId.Value,
                "NewApplication",
                $"{candidate?.Name} applied to '{job.Title}'",
                $"/Application/Detail/{app.Id}");
        }

        return app.Id;
    }

    public async Task<List<ApplicationListItemViewModel>> GetApplicationsByUserAsync(int userId)
        => await _db.Applications
            .Where(a => a.UserId == userId)
            .Include(a => a.JobOffer).ThenInclude(j => j!.Company)
            .Include(a => a.Interview)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new ApplicationListItemViewModel
            {
                Id          = a.Id,
                JobTitle    = a.JobOffer!.Title,
                CompanyName = a.JobOffer.Company != null ? a.JobOffer.Company.Name : "Independent",
                Status      = a.Status,
                MatchScore  = a.MatchScore,
                AppliedAt   = a.AppliedAt,
                ResumePath  = a.ResumePath,
                InterviewId = a.Interview != null ? a.Interview.Id : null
            })
            .ToListAsync();

    public async Task<List<ApplicationListItemViewModel>> GetApplicationsByJobAsync(int jobOfferId)
        => await _db.Applications
            .Where(a => a.JobOfferId == jobOfferId)
            .Include(a => a.User)
            .Include(a => a.Interview)
            .OrderByDescending(a => a.MatchScore)
            .Select(a => new ApplicationListItemViewModel
            {
                Id             = a.Id,
                JobTitle       = a.JobOffer!.Title,
                CompanyName    = string.Empty,
                Status         = a.Status,
                MatchScore     = a.MatchScore,
                AppliedAt      = a.AppliedAt,
                CandidateName  = a.User!.Name,
                CandidateEmail = a.User.Email,
                ResumePath     = a.ResumePath,
                InterviewId    = a.Interview != null ? a.Interview.Id : null
            })
            .ToListAsync();

    public async Task<List<ApplicationListItemViewModel>> GetApplicationsByRecruiterAsync(int recruiterUserId)
    {
        var jobIds = await _db.JobOffers
            .Where(j => j.PostedByUserId == recruiterUserId)
            .Select(j => j.Id)
            .ToListAsync();

        return await _db.Applications
            .Where(a => jobIds.Contains(a.JobOfferId))
            .Include(a => a.User)
            .Include(a => a.JobOffer).ThenInclude(j => j!.Company)
            .Include(a => a.Interview)
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
                ResumePath     = a.ResumePath,
                InterviewId    = a.Interview != null ? a.Interview.Id : null
            })
            .ToListAsync();
    }

    public async Task<ApplicationDetailViewModel?> GetApplicationDetailAsync(int applicationId)
    {
        var a = await _db.Applications
            .Include(x => x.User).ThenInclude(u => u!.CandidateProfile)
            .Include(x => x.JobOffer).ThenInclude(j => j!.Company)
            .Include(x => x.Interview)
            .FirstOrDefaultAsync(x => x.Id == applicationId);

        if (a == null) return null;

        return new ApplicationDetailViewModel
        {
            Id                    = a.Id,
            JobOfferId            = a.JobOfferId,
            JobTitle              = a.JobOffer!.Title,
            CompanyName           = a.JobOffer.Company?.Name ?? "Independent",
            CandidateName         = a.User!.Name,
            CandidateEmail        = a.User.Email,
            CandidateHeadline     = a.User.CandidateProfile?.Headline,
            CoverLetter           = a.CoverLetter,
            ResumePath            = a.ResumePath,
            Status                = a.Status,
            MatchScore            = a.MatchScore,
            AppliedAt             = a.AppliedAt,
            InterviewScheduledAt  = a.Interview?.ScheduledAt,
            InterviewMeetingLink  = a.Interview?.MeetingLink,
            InterviewNotes        = a.Interview?.Notes
        };
    }

    public async Task<bool> UpdateStatusAsync(int applicationId, string status)
    {
        var app = await _db.Applications
            .Include(a => a.User)
            .Include(a => a.JobOffer)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (app == null) return false;

        app.Status = status;
        await _db.SaveChangesAsync();

        // Notify candidate
        if (app.User != null)
        {
            await _email.SendEmailAsync(
                app.User.Email,
                $"Application Update: {app.JobOffer?.Title}",
                $"<p>Dear {app.User.Name},</p><p>Your application status has been updated to: <strong>{status}</strong>.</p>");
        }

        return true;
    }

    public async Task<bool> ScheduleInterviewAsync(ScheduleInterviewViewModel model)
    {
        var app = await _db.Applications
            .Include(a => a.User)
            .Include(a => a.JobOffer)
            .Include(a => a.Interview)
            .FirstOrDefaultAsync(a => a.Id == model.ApplicationId);

        if (app == null) return false;

        if (app.Interview == null)
        {
            app.Interview = new Interview
            {
                ApplicationId = app.Id,
                ScheduledAt   = model.ScheduledAt,
                MeetingLink   = model.MeetingLink,
                Notes         = model.Notes,
                Status        = "Scheduled"
            };
        }
        else
        {
            app.Interview.ScheduledAt = model.ScheduledAt;
            app.Interview.MeetingLink = model.MeetingLink;
            app.Interview.Notes       = model.Notes;
        }

        app.Status = "InterviewScheduled";
        await _db.SaveChangesAsync();

        // Email candidate
        if (app.User != null)
        {
            await _email.SendEmailAsync(
                app.User.Email,
                $"Interview Scheduled: {app.JobOffer?.Title}",
                $@"<p>Dear {app.User.Name},</p>
                   <p>Your interview for <strong>{app.JobOffer?.Title}</strong> has been scheduled.</p>
                   <p><strong>Date:</strong> {model.ScheduledAt:dddd, MMMM dd yyyy HH:mm}</p>
                   <p><strong>Link:</strong> <a href=""{model.MeetingLink}"">{model.MeetingLink}</a></p>
                   {(string.IsNullOrEmpty(model.Notes) ? "" : $"<p><strong>Notes:</strong> {model.Notes}</p>")}");
        }

        return true;
    }
}
