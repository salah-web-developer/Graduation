
namespace Graduation.Dtos.Users
{
    public class CreateDoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int MaxProjects { get; set; }
    }
}
