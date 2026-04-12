using System.ComponentModel.DataAnnotations.Schema;

namespace recrutementapp.Models;

public class JobSkill
{
    public int JobOfferId { get; set; }

    [ForeignKey(nameof(JobOfferId))]
    public JobOffer JobOffer { get; set; } = null!;

    public int SkillId { get; set; }

    [ForeignKey(nameof(SkillId))]
    public Skill Skill { get; set; } = null!;

    public bool IsRequired { get; set; } = true;
}
