using Microsoft.EntityFrameworkCore;
using recrutementapp.Models;

namespace recrutementapp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<RecruiterProfile> RecruiterProfiles => Set<RecruiterProfile>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobOffer> JobOffers => Set<JobOffer>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Composite PKs for junction tables
        modelBuilder.Entity<JobSkill>()
            .HasKey(js => new { js.JobOfferId, js.SkillId });

        modelBuilder.Entity<CandidateSkill>()
            .HasKey(cs => new { cs.CandidateProfileId, cs.SkillId });

        // Application → User (restrict to avoid cascade conflict)
        modelBuilder.Entity<Application>()
            .HasOne(a => a.User)
            .WithMany(u => u.Applications)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Application → JobOffer (cascade)
        modelBuilder.Entity<Application>()
            .HasOne(a => a.JobOffer)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Interview → Application (cascade)
        modelBuilder.Entity<Interview>()
            .HasOne(i => i.Application)
            .WithOne(a => a.Interview)
            .HasForeignKey<Interview>(i => i.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        // Prevent duplicate application
        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.UserId, a.JobOfferId }).IsUnique();

        // Performance indexes
        modelBuilder.Entity<JobOffer>().HasIndex(j => j.Status);
        modelBuilder.Entity<Notification>().HasIndex(n => new { n.UserId, n.IsRead });
    }
}
