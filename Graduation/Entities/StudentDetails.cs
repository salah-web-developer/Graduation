namespace Graduation.Entities
{
    public class StudentDetails
    {
        public int Id { get; set; }
        public double GPA { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
