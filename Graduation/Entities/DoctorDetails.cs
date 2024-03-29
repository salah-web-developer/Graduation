namespace Graduation.Entities
{
    public class DoctorDetails
    {
        public int Id { get; set; }
        public int MaxProjects { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
