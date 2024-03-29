
namespace Graduation.Dtos.ProjectIdeaRequests
{
    public class CreateProjectIdeaRequestDto
    {
        public string Title { get; set; }
        public IFormFile File { get; set; }
        public int DoctorId { get; set; }
        public int AssistantDoctorId { get; set; }
        public ICollection<int> StudentsIds { get; set; }
    }
}
