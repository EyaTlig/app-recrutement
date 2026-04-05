using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recrutementapp.Models;

public class CandidateProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [StringLength(200)]
    public string? Headline { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    public string? Summary { get; set; }

    public int ExperienceYears { get; set; }

    [StringLength(200)]
    public string? LinkedInUrl { get; set; }

    [StringLength(200)]
    public string? GitHubUrl { get; set; }

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
}
