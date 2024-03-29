using AutoMapper;
using Graduation.Dtos.ProjectIdeaRequests;
using Graduation.Dtos.Teams;
using Graduation.Dtos.Users;
using Graduation.Entities;

namespace Graduation.Mappings
{
    public class Profiles : Profile
    {
        public Profiles()
        {
            CreateMap<User, UserDto>();
            CreateMap<ProjectIdeaRequest, ProjectIdeaRequestDto>();
            CreateMap<Project, ProjectDto>();
            CreateMap<DoctorDetails, DoctorDetailsDto>();
            CreateMap<StudentDetails, StudentDetailsDto>();
                
                
        }
    }
}
