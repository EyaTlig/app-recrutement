using System.ComponentModel.DataAnnotations;

namespace recrutementapp.ViewModels;

public class JobListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoPath { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsRemote { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ApplicationCount { get; set; }
    public int? MatchScore { get; set; }
    public bool HasApplied { get; set; }
    public List<string> RequiredSkills { get; set; } = new();
}

public class JobDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoPath { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanyDescription { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsRemote { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> RequiredSkills { get; set; } = new();
    public List<string> PreferredSkills { get; set; } = new();
    public bool HasApplied { get; set; }
    public int? MatchScore { get; set; }
}

public class CreateJobViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    public bool IsRemote { get; set; }

    [Required]
    [Display(Name = "Contract Type")]
    public string ContractType { get; set; } = "FullTime";

    [Display(Name = "Min Salary (TND)")]
    public decimal? SalaryMin { get; set; }

    [Display(Name = "Max Salary (TND)")]
    public decimal? SalaryMax { get; set; }

    [Display(Name = "Expiry Date")]
    public DateTime? ExpiresAt { get; set; }

    public List<int> RequiredSkillIds { get; set; } = new();
    public List<int> PreferredSkillIds { get; set; } = new();
}
