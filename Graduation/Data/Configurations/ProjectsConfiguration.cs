using Graduation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Graduation.Data.Configurations
{
    public class ProjectsConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasOne(r => r.Doctor).WithMany().HasForeignKey(r => r.DoctorId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(r => r.AssistantDoctor).WithMany().HasForeignKey(r => r.AssistantDoctorId).OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(r => r.Students).WithMany();
            builder.HasOne(r => r.TeamLeader).WithMany().HasForeignKey(r => r.TeamLeaderId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
