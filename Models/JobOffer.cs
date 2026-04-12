using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recrutementapp.Models;

public class JobOffer
{
    [Key]
    public int Id { get; set; }

    public int? CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public int? PostedByUserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    public bool IsRemote { get; set; }

    /// <summary>FullTime | PartTime | Freelance | Internship</summary>
    public string ContractType { get; set; } = "FullTime";

    /// <summary>Active | Paused | Closed</summary>
    public string Status { get; set; } = "Active";

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}
