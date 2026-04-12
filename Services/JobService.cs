using Microsoft.EntityFrameworkCore;
using recrutementapp.Data;
using recrutementapp.Models;
using recrutementapp.ViewModels;

namespace recrutementapp.Services;

public class JobService : IJobService
{
    private readonly ApplicationDbContext _db;

    public JobService(ApplicationDbContext db) => _db = db;

    public async Task<List<JobListItemViewModel>> GetActiveJobsAsync(
        string? search, string? location, string? contractType, int page, int pageSize)
    {
        var query = _db.JobOffers
            .Where(j => j.Status == "Active" && (j.ExpiresAt == null || j.ExpiresAt > DateTime.UtcNow))
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(j => j.Applications)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(j => j.Title.Contains(search) ||
                                     j.Description.Contains(search) ||
                                     (j.Company != null && j.Company.Name.Contains(search)));

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(j => j.Location.Contains(location) || j.IsRemote);

        if (!string.IsNullOrWhiteSpace(contractType))
            query = query.Where(j => j.ContractType == contractType);

        return await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobListItemViewModel
            {
                Id               = j.Id,
                Title            = j.Title,
                CompanyName      = j.Company != null ? j.Company.Name : "Independent",
                CompanyLogoPath  = j.Company != null ? j.Company.LogoPath : null,
                Location         = j.Location,
                IsRemote         = j.IsRemote,
                ContractType     = j.ContractType,
                SalaryMin        = j.SalaryMin,
                SalaryMax        = j.SalaryMax,
                CreatedAt        = j.CreatedAt,
                ApplicationCount = j.Applications.Count,
                RequiredSkills   = j.JobSkills
                    .Where(js => js.IsRequired)
                    .Select(js => js.Skill.Name)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<int> GetActiveJobsCountAsync(string? search, string? location, string? contractType)
    {
        var query = _db.JobOffers
            .Where(j => j.Status == "Active" && (j.ExpiresAt == null || j.ExpiresAt > DateTime.UtcNow))
            .Include(j => j.Company)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(j => j.Title.Contains(search) ||
                                     (j.Company != null && j.Company.Name.Contains(search)));

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(j => j.Location.Contains(location) || j.IsRemote);

        if (!string.IsNullOrWhiteSpace(contractType))
            query = query.Where(j => j.ContractType == contractType);

        return await query.CountAsync();
    }

    public async Task<JobDetailViewModel?> GetJobDetailAsync(int id, int? currentUserId)
    {
        var job = await _db.JobOffers
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return null;

        bool hasApplied = currentUserId.HasValue &&
            job.Applications.Any(a => a.UserId == currentUserId.Value);

        return new JobDetailViewModel
        {
            Id                 = job.Id,
            Title              = job.Title,
            Description        = job.Description,
            CompanyName        = job.Company?.Name ?? "Independent",
            CompanyLogoPath    = job.Company?.LogoPath,
            CompanyWebsite     = job.Company?.Website,
            CompanyDescription = job.Company?.Description,
            Location           = job.Location,
            IsRemote           = job.IsRemote,
            ContractType       = job.ContractType,
            SalaryMin          = job.SalaryMin,
            SalaryMax          = job.SalaryMax,
            CreatedAt          = job.CreatedAt,
            ExpiresAt          = job.ExpiresAt,
            RequiredSkills     = job.JobSkills.Where(js => js.IsRequired).Select(js => js.Skill.Name).ToList(),
            PreferredSkills    = job.JobSkills.Where(js => !js.IsRequired).Select(js => js.Skill.Name).ToList(),
            HasApplied         = hasApplied
        };
    }

    public async Task<JobOffer> CreateJobAsync(CreateJobViewModel model, int companyId, int postedByUserId)
    {
        var job = new JobOffer
        {
            CompanyId      = companyId,
            PostedByUserId = postedByUserId,
            Title          = model.Title,
            Description    = model.Description,
            Location       = model.IsRemote ? "Remote" : model.Location,
            IsRemote       = model.IsRemote,
            ContractType   = model.ContractType,
            SalaryMin      = model.SalaryMin,
            SalaryMax      = model.SalaryMax,
            ExpiresAt      = model.ExpiresAt,
            Status         = "Active"
        };

        foreach (var sid in model.RequiredSkillIds)
            job.JobSkills.Add(new JobSkill { SkillId = sid, IsRequired = true });
        foreach (var sid in model.PreferredSkillIds)
            job.JobSkills.Add(new JobSkill { SkillId = sid, IsRequired = false });

        _db.JobOffers.Add(job);
        await _db.SaveChangesAsync();
        return job;
    }

    public async Task<List<JobListItemViewModel>> GetJobsByUserAsync(int userId)
    {
        return await _db.JobOffers
            .Where(j => j.PostedByUserId == userId)
            .Include(j => j.Company)
            .Include(j => j.Applications)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobListItemViewModel
            {
                Id               = j.Id,
                Title            = j.Title,
                CompanyName      = j.Company != null ? j.Company.Name : "Independent",
                Location         = j.Location,
                ContractType     = j.ContractType,
                CreatedAt        = j.CreatedAt,
                ApplicationCount = j.Applications.Count
            })
            .ToListAsync();
    }

    public async Task<bool> CloseJobAsync(int id, int currentUserId)
    {
        var job = await _db.JobOffers
            .FirstOrDefaultAsync(j => j.Id == id && j.PostedByUserId == currentUserId);
        if (job == null) return false;
        job.Status = "Closed";
        await _db.SaveChangesAsync();
        return true;
    }
}
