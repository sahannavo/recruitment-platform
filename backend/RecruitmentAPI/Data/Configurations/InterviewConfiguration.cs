using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Interview entity
/// </summary>
public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.HasKey(i => i.InterviewId);

        builder.Property(i => i.ScheduledAt)
            .IsRequired();

        builder.Property(i => i.Duration)
            .IsRequired();

        builder.Property(i => i.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Scheduled");

        builder.Property(i => i.MeetingLink)
            .HasMaxLength(500);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(i => i.UpdatedAt);

        // Relationships
        builder.HasOne(i => i.Application)
            .WithMany()
            .HasForeignKey(i => i.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Feedbacks)
            .WithOne(f => f.Interview)
            .HasForeignKey(f => f.InterviewId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => i.ApplicationId);
        builder.HasIndex(i => i.ScheduledAt);
        builder.HasIndex(i => i.Status);
    }
}
