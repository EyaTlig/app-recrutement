using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace recrutementapp.ViewModels;

public class ApplyViewModel
{
    [Required]
    public int JobOfferId { get; set; }

    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;

    [Display(Name = "Cover Letter")]
    [StringLength(3000)]
    public string? CoverLetter { get; set; }

    [Display(Name = "Resume (PDF)")]
    public IFormFile? ResumeFile { get; set; }
}

public class ApplicationListItemViewModel
{
    public int Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int MatchScore { get; set; }
    public DateTime AppliedAt { get; set; }
    public string? CandidateName { get; set; }
    public string? CandidateEmail { get; set; }
    public string? ResumePath { get; set; }
    public int? InterviewId { get; set; }
}

public class ApplicationDetailViewModel
{
    public int Id { get; set; }
    public int JobOfferId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidateHeadline { get; set; }
    public string? CoverLetter { get; set; }
    public string? ResumePath { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MatchScore { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? InterviewScheduledAt { get; set; }
    public string? InterviewMeetingLink { get; set; }
    public string? InterviewNotes { get; set; }
}

public class ScheduleInterviewViewModel
{
    [Required]
    public int ApplicationId { get; set; }

    [Required]
    [Display(Name = "Interview Date & Time")]
    public DateTime ScheduledAt { get; set; } = DateTime.Now.AddDays(3);

    [Required]
    [Display(Name = "Meeting Link")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string MeetingLink { get; set; } = string.Empty;

    [Display(Name = "Notes for Candidate")]
    public string? Notes { get; set; }
}
