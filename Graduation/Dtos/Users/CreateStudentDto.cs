namespace Graduation.Dtos.Users
{
    public class CreateStudentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public double GPA { get; set; }
    }
}
