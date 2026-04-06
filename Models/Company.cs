using System.ComponentModel.DataAnnotations;

namespace recrutementapp.Models;

public class Company
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Industry { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? Website { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    public string? LogoPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<JobOffer> JobOffers { get; set; } = new List<JobOffer>();
    public ICollection<RecruiterProfile> Recruiters { get; set; } = new List<RecruiterProfile>();
}
