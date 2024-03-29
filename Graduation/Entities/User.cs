using Graduation.Enums;

namespace Graduation.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HashedPassword { get; set; }

        public Roles Role { get; set; }
        public DoctorDetails DoctorDetails { get; set; }
        public StudentDetails StudentDetails { get; set; }
    }
}
