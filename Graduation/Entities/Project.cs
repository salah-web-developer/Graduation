﻿
namespace Graduation.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FileUrl { get; set; }
        public int TeamLeaderId { get; set; }
        public int DoctorId { get; set; }
        public int AssistantDoctorId { get; set; }

        public User TeamLeader { get; set; }
        public ICollection<User> Students { get; set; }
        public User Doctor { get; set; }
        public User AssistantDoctor { get; set; }
    }
}
