
namespace Graduation.Dtos.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public DoctorDetailsDto DoctorDetails { get; set; }
        public StudentDetailsDto StudentDetails { get; set; }
    }
}
