using Graduation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Graduation.Data.Configurations
{
    public class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.HasOne(u => u.DoctorDetails)
                .WithOne(d=>d.User)
                .HasForeignKey<DoctorDetails>(d=>d.UserId);

            builder.Navigation(u => u.DoctorDetails).AutoInclude();
            builder.Navigation(u => u.StudentDetails).AutoInclude();
        }
    }
}
