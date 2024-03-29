using Graduation.Dtos.Users;

namespace Graduation.Dtos.ProjectIdeaRequests
{
    public class ProjectIdeaRequestDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string FileUrl { get; set; }
        public DateTime RequestedOn { get; set; }

        public UserDto TeamLeader { get; set; }
        public ICollection<UserDto> Students { get; set; }
        public UserDto Doctor { get; set; }
        public UserDto AssistantDoctor { get; set; }
    }
}
