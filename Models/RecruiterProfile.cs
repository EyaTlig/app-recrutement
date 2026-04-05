using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recrutementapp.Models;

public class RecruiterProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public int? CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    [StringLength(100)]
    public string? Title { get; set; }

    [StringLength(100)]
    public string? Phone { get; set; }
}
