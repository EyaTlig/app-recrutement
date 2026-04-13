using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recrutementapp.Models;

public class Application
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int JobOfferId { get; set; }

    [ForeignKey(nameof(JobOfferId))]
    public JobOffer? JobOffer { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    /// Pending | Reviewed | Shortlisted | InterviewScheduled | Accepted | Rejected 
    public string Status { get; set; } = "Pending";

    public string? CoverLetter { get; set; }

    public string? ResumePath { get; set; }

    public int MatchScore { get; set; } = 0;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public Interview? Interview { get; set; }
}
