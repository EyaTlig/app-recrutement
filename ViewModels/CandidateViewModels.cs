using System.ComponentModel.DataAnnotations;

namespace recrutementapp.ViewModels;

public class CandidateProfileViewModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Professional Headline")]
    public string? Headline { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [Display(Name = "Professional Summary")]
    public string? Summary { get; set; }

    [Range(0, 50)]
    [Display(Name = "Years of Experience")]
    public int ExperienceYears { get; set; }

    [Url]
    [StringLength(200)]
    [Display(Name = "LinkedIn URL")]
    public string? LinkedInUrl { get; set; }

    [Url]
    [StringLength(200)]
    [Display(Name = "GitHub URL")]
    public string? GitHubUrl { get; set; }

    public List<CandidateSkillViewModel> Skills { get; set; } = new();
    public List<ApplicationListItemViewModel> RecentApplications { get; set; } = new();
}

public class CandidateSkillViewModel
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int ProficiencyLevel { get; set; }
}

public class DashboardViewModel
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int InterviewsScheduled { get; set; }
    public List<ApplicationListItemViewModel> RecentApplications { get; set; } = new();
    public List<JobListItemViewModel> RecommendedJobs { get; set; } = new();
}

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalJobs { get; set; }
    public int TotalApplications { get; set; }
    public int TotalCompanies { get; set; }
    public int PendingApplications { get; set; }
    public int ActiveJobs { get; set; }
    public List<ApplicationListItemViewModel> RecentApplications { get; set; } = new();
}

public class RecruiterDashboardViewModel
{
    public int TotalJobOffers { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int InterviewsScheduled { get; set; }
    public List<ApplicationListItemViewModel> RecentApplications { get; set; } = new();
    public List<JobListItemViewModel> MyJobs { get; set; } = new();
}
