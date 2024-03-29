using AutoMapper;
using AutoMapper.QueryableExtensions;
using Graduation.Attributes;
using Graduation.Data;
using Graduation.Dtos.Projects;
using Graduation.Dtos.Teams;
using Graduation.Entities;
using Graduation.Enums;
using Graduation.Extensions;
using Graduation.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation.Controllers
{
    [Route("api/projects")]
    public class ProjectsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> GetAllAsync(int? doctorId, int? assistantDoctorId)
        {
            var query = _context.Projects.AsQueryable();

            if(doctorId != null)
                query = query.Where(t=>t.DoctorId == doctorId);
            
            if(assistantDoctorId != null)
                query = query.Where(t=>t.AssistantDoctorId == assistantDoctorId);

            return Ok(await query
                .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
                .ToListAsync());
        }
        
        [HttpGet("id")]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var project = await _context.Projects
                .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(p=>p.Id == id);

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        [HttpDelete("id")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("doctorProjects")]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        [HasRole(Roles.Doctor)]
        public async Task<IActionResult> GetAllDoctorProjectsAsync()
        {
            var doctorId = User.Id();

            var projects = await _context.Projects
                .Where(t => t.DoctorId == doctorId || t.AssistantDoctorId == doctorId)
                .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("studentProject")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [HasRole(Roles.Student)]
        public async Task<IActionResult> GetStudentProjectAsync()
        {
            var studentId = User.Id();

            var projects = await _context.Projects
                .Where(t => t.TeamLeaderId == studentId || t.Students.Any(s=>s.Id == studentId))
                .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return Ok(projects);
        }


        [HttpPost("isStudentInProject")]
        [ProducesResponseType(typeof(bool), 200)]
        [Authorize]
        public async Task<IActionResult> IsStudentInProjectAsync(DeleteProjectDto request)
        {
            return Ok(await _context.Projects.AnyAsync(t=>t.Students.Any(s=>s.Id == request.Id)));
        }

        [HttpPut]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> UpdateAsync(UpdateProjectDto request)
        {
            var project = await _context.Projects
                .Include(t=>t.Students)
                .FirstOrDefaultAsync(t => t.Id == request.Id);

            if (project == null)
                return NotFound();

            var students = await _context.Users
                .Where(u => request.StudentsIds.Any(i => i == u.Id) && u.Role == Roles.Student)
                .ToListAsync();

            await ValidateProjectIdeaRequestAsync(request, students);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            project.Title = request.Title;
            project.DoctorId = request.DoctorId;
            project.TeamLeaderId = request.TeamLeaderId;
            project.AssistantDoctorId = request.AssistantDoctorId;
            project.Students = students;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPut("updateProjectFile")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> UpdateProjectFileAsync([FromForm] UpdateProjectFileDto request)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(t => t.Id == request.Id);

            FileHelpers.Remove(project.FileUrl);
            await FileHelpers.SaveAsync(request.File, project.FileUrl);

            return NoContent();
        }

        private async Task ValidateProjectIdeaRequestAsync(UpdateProjectDto request, List<User> students)
        {
            if (students.Count() != request.StudentsIds.Count())
                ModelState.AddModelError(nameof(request.StudentsIds), "Students Ids value is invalid.");

            if (await _context.Projects.AnyAsync(t => t.Id != request.Id && t.Students.Any(s => request.StudentsIds.Any(i => i == s.Id))))
                ModelState.AddModelError(nameof(request.StudentsIds), "One or more student is in a project.");

            if (!await _context.Users.AnyAsync(u => u.Id == request.DoctorId && u.Role == Roles.Doctor))
                ModelState.AddModelError(nameof(request.DoctorId), "Doctor Id is invalid.");

            if (!await _context.Users.AnyAsync(u => u.Id == request.AssistantDoctorId && u.Role == Roles.Doctor))
                ModelState.AddModelError(nameof(request.AssistantDoctorId), "Assistant Doctor Id is invalid.");
        }

        [HttpPost("getDoctorProjectsCount")]
        [ProducesResponseType(typeof(int), 200)]
        [Authorize]
        public async Task<IActionResult> GetDoctorProjectsCountAsync(DeleteProjectDto request)
        {
            return Ok(await _context.Projects.CountAsync(t => t.DoctorId == request.Id));
        }
    }
}
