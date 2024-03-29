using AutoMapper;
using AutoMapper.QueryableExtensions;
using Graduation.Attributes;
using Graduation.Data;
using Graduation.Dtos.ProjectIdeaRequests;
using Graduation.Entities;
using Graduation.Enums;
using Graduation.Extensions;
using Graduation.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation.Controllers
{
    [Route("api/projectIdeaRequests")]
    [Authorize]
    public class ProjectIdeaRequestsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectIdeaRequestsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), 200)]
        [HasRole(Roles.Student)]
        public async Task<IActionResult> CreateAsync([FromForm] CreateProjectIdeaRequestDto request)
        {
            var currentUserId = User.Id()!.Value;

            if (!request.StudentsIds.Any(i=> i == currentUserId))
                request.StudentsIds.Add(currentUserId);

            var students = await _context.Users
                .Where(u => request.StudentsIds.Contains(u.Id) && u.Role == Roles.Student)
                .ToListAsync();

            await ValidateProjectIdeaRequestAsync(request, students);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var projectRequest = new ProjectIdeaRequest
            {
                Title = request.Title,
                Status = ProjectIdeaRequestStatuses.Pending,
                FileUrl = await FileHelpers.SaveAsync(request.File),
                TeamLeaderId = currentUserId,
                DoctorId = request.DoctorId,
                AssistantDoctorId = request.AssistantDoctorId,
                Students = students,
                RequestedOn = DateTime.Now
            };

            _context.ProjectIdeaRequests.Add(projectRequest);
            await _context.SaveChangesAsync();

            return Ok(projectRequest.Id);
        }

        private async Task ValidateProjectIdeaRequestAsync(CreateProjectIdeaRequestDto request, List<User> students)
        {
            if (students.Count() != request.StudentsIds.Count())
                ModelState.AddModelError(nameof(request.StudentsIds), "Students Ids value is invalid.");

            if (await _context.Projects.AnyAsync(t => t.Students.Any(s => request.StudentsIds.Any(i => i == s.Id))))
                ModelState.AddModelError(nameof(request.StudentsIds), "One or more student is in a project.");

            if (!await _context.Users.AnyAsync(u => u.Id == request.DoctorId && u.Role == Roles.Doctor))
                ModelState.AddModelError(nameof(request.DoctorId), "Doctor Id is invalid.");

            if (!await _context.Users.AnyAsync(u => u.Id == request.AssistantDoctorId && u.Role == Roles.Doctor))
                ModelState.AddModelError(nameof(request.AssistantDoctorId), "Assistant Doctor Id is invalid.");
        }

        [HttpGet("doctorRequests")]
        [ProducesResponseType(typeof(IEnumerable<ProjectIdeaRequestDto>), 200)]
        [HasRole(Roles.Doctor)]
        public async Task<IActionResult> GetAllForDoctorAsync(string? status)
        {
            var query = _context.ProjectIdeaRequests
                .Where(r => r.DoctorId == User.Id())
                .OrderByDescending(r=>r.RequestedOn)
                .AsQueryable();

            if (status != null && Enum.IsDefined(typeof(ProjectIdeaRequestStatuses), status))
                query = query.Where(r => r.Status == Enum.Parse<ProjectIdeaRequestStatuses>(status));

            return Ok(await query
                .ProjectTo<ProjectIdeaRequestDto>(_mapper.ConfigurationProvider)
                .ToListAsync());
        }
        
        [HttpGet("studentRequests")]
        [ProducesResponseType(typeof(IEnumerable<ProjectIdeaRequestDto>), 200)]
        [HasRole(Roles.Student)]
        public async Task<IActionResult> GetAllForStudentAsync(string? status)
        {
            var query = _context.ProjectIdeaRequests
                .Where(r => r.TeamLeaderId == User.Id())
                .OrderByDescending(r => r.RequestedOn)
                .AsQueryable();

            if (status != null && Enum.IsDefined(typeof(ProjectIdeaRequestStatuses), status))
                query = query.Where(r => r.Status == Enum.Parse<ProjectIdeaRequestStatuses>(status));

            return Ok(await query
                .ProjectTo<ProjectIdeaRequestDto>(_mapper.ConfigurationProvider)
                .ToListAsync());
        }

        [HttpPut("rejectIdea")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Doctor)]
        public async Task<IActionResult> RejectIdeaAsync(UpdateProjectIdeaRequestStatusDto request )
        {
            var projectIdeaRequest = await _context.ProjectIdeaRequests.FindAsync(request.Id);

            if (projectIdeaRequest == null)
                return NotFound();

            if (projectIdeaRequest.DoctorId != User.Id())
                return StatusCode(403, "Only owner doctor can accept this request.");

            if (projectIdeaRequest.Status != ProjectIdeaRequestStatuses.Pending)
                return Conflict("Idea is already " + projectIdeaRequest.Status);

            projectIdeaRequest.Status = ProjectIdeaRequestStatuses.Rejected;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPut("acceptIdea")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Doctor)]
        public async Task<IActionResult> AcceptIdeaAsync(UpdateProjectIdeaRequestStatusDto request)
        {
            var projectIdeaRequest = await _context.ProjectIdeaRequests
                .Include(r=>r.Doctor)
                .Include(r => r.Students)
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (projectIdeaRequest == null)
                return NotFound();

            var docId = User.Id().Value;

            if (projectIdeaRequest.DoctorId != docId)
                return StatusCode(403, "Only owner doctor can accept this request.");

            if (projectIdeaRequest.Status != ProjectIdeaRequestStatuses.Pending)
                return Conflict("Idea is already " + projectIdeaRequest.Status);

            var doctorProjectsCount = await _context.Projects.CountAsync(t => t.DoctorId == docId);

            if (doctorProjectsCount >= projectIdeaRequest.Doctor.DoctorDetails.MaxProjects)
                return StatusCode(403, "Can not accept any more projects you have reached to the max projects count.");

            CreateTeam(projectIdeaRequest);
            AcceptRequest(projectIdeaRequest);
            await MarkRelatedRequestsAsMissed(projectIdeaRequest);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task MarkRelatedRequestsAsMissed(ProjectIdeaRequest projectIdeaRequest)
        {
            var relatedRequests = await _context.ProjectIdeaRequests
                            .Where(r1 => r1.Status == ProjectIdeaRequestStatuses.Pending &&
                                    r1.Students.Any(s1 => _context.ProjectIdeaRequests.Any(r2 => 
                                    r2.Id == projectIdeaRequest.Id &&
                                    r1.Id != r2.Id &&
                                    r2.Students.Any(s2 => s2.Id == s1.Id))))
                            .ToListAsync();

            relatedRequests.ForEach(r => r.Status = ProjectIdeaRequestStatuses.Missed);
        }

        private void AcceptRequest(ProjectIdeaRequest? projectIdeaRequest)
        {
            projectIdeaRequest.Status = ProjectIdeaRequestStatuses.Accepted;
        }

        private void CreateTeam(ProjectIdeaRequest? projectIdeaRequest)
        {
            var project = new Project
            {
                Title = projectIdeaRequest.Title,
                FileUrl = projectIdeaRequest.FileUrl,
                TeamLeaderId = projectIdeaRequest.TeamLeaderId,
                DoctorId = projectIdeaRequest.DoctorId,
                AssistantDoctorId = projectIdeaRequest.AssistantDoctorId,
                Students = projectIdeaRequest.Students
            };

            _context.Add(project);
        }
    }
}
