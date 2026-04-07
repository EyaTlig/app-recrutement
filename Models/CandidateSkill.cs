using System.ComponentModel.DataAnnotations.Schema;

namespace recrutementapp.Models;

public class CandidateSkill
{
    public int CandidateProfileId { get; set; }

    [ForeignKey(nameof(CandidateProfileId))]
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public int SkillId { get; set; }

    [ForeignKey(nameof(SkillId))]
    public Skill Skill { get; set; } = null!;

    /// 1=Beginner … 5=Expert
    public int ProficiencyLevel { get; set; } = 1;
}
