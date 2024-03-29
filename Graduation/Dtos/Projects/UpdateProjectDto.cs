namespace Graduation.Dtos.Teams
{
    public class UpdateProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TeamLeaderId { get; set; }
        public int DoctorId { get; set; }
        public int AssistantDoctorId { get; set; }
        public ICollection<int> StudentsIds { get; set; }
    }
}
