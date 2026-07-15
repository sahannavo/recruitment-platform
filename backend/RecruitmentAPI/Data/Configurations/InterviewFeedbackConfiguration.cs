using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Data.Configurations;

/// <summary>
/// Entity Framework configuration for InterviewFeedback entity
/// </summary>
public class InterviewFeedbackConfiguration : IEntityTypeConfiguration<InterviewFeedback>
{
    public void Configure(EntityTypeBuilder<InterviewFeedback> builder)
    {
        builder.HasKey(f => f.FeedbackId);

        builder.Property(f => f.TechnicalScore)
            .IsRequired()
            .HasColumnType("decimal(3,1)");

        builder.Property(f => f.BehavioralScore)
            .IsRequired()
            .HasColumnType("decimal(3,1)");

        builder.Property(f => f.CommunicationScore)
            .IsRequired()
            .HasColumnType("decimal(3,1)");

        builder.Property(f => f.Comments)
            .HasMaxLength(2000);

        builder.Property(f => f.Decision)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(f => f.UpdatedAt);

        // Relationships
        builder.HasOne(f => f.Interview)
            .WithMany(i => i.Feedbacks)
            .HasForeignKey(f => f.InterviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Manager)
            .WithMany()
            .HasForeignKey(f => f.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(f => f.InterviewId);
        builder.HasIndex(f => f.ManagerId);
        builder.HasIndex(f => f.Decision);

        // Unique constraint - one feedback per interview per manager
        builder.HasIndex(f => new { f.InterviewId, f.ManagerId })
            .IsUnique();
    }
}
