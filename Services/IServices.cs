using recrutementapp.Models;
using recrutementapp.ViewModels;

namespace recrutementapp.Services;

// ─── Email ───────────────────────────────────────────────────────────────────
public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}

// ─── File Storage ─────────────────────────────────────────────────────────────
public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    void DeleteFile(string relativePath);
}

// ─── Jobs ─────────────────────────────────────────────────────────────────────
public interface IJobService
{
    Task<List<JobListItemViewModel>> GetActiveJobsAsync(string? search, string? location, string? contractType, int page, int pageSize);
    Task<int> GetActiveJobsCountAsync(string? search, string? location, string? contractType);
    Task<JobDetailViewModel?> GetJobDetailAsync(int id, int? currentUserId);
    Task<JobOffer> CreateJobAsync(CreateJobViewModel model, int companyId, int postedByUserId);
    Task<List<JobListItemViewModel>> GetJobsByUserAsync(int userId);
    Task<bool> CloseJobAsync(int id, int currentUserId);
}

// ─── Applications ─────────────────────────────────────────────────────────────
public interface IApplicationService
{
    Task<bool> HasAppliedAsync(int userId, int jobOfferId);
    Task<int> SubmitApplicationAsync(ApplyViewModel model, int userId);
    Task<List<ApplicationListItemViewModel>> GetApplicationsByUserAsync(int userId);
    Task<List<ApplicationListItemViewModel>> GetApplicationsByJobAsync(int jobOfferId);
    Task<List<ApplicationListItemViewModel>> GetApplicationsByRecruiterAsync(int recruiterUserId);
    Task<ApplicationDetailViewModel?> GetApplicationDetailAsync(int applicationId);
    Task<bool> UpdateStatusAsync(int applicationId, string status);
    Task<bool> ScheduleInterviewAsync(ScheduleInterviewViewModel model);
}

// ─── Notifications ────────────────────────────────────────────────────────────
public interface INotificationService
{
    Task CreateAsync(int userId, string type, string content, string? link = null);
    Task<List<Notification>> GetUnreadAsync(int userId);
    Task MarkAllReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
}
