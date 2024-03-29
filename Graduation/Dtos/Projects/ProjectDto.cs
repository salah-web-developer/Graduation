using Graduation.Dtos.Users;

namespace Graduation.Dtos.Teams
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FileUrl { get; set; }

        public UserDto TeamLeader { get; set; }
        public ICollection<UserDto> Students { get; set; }
        public UserDto Doctor { get; set; }
        public UserDto AssistantDoctor { get; set; }
    }
}
