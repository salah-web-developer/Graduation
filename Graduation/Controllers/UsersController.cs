using AutoMapper;
using AutoMapper.QueryableExtensions;
using Graduation.Attributes;
using Graduation.Data;
using Graduation.Dtos.Users;
using Graduation.Entities;
using Graduation.Enums;
using Graduation.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Graduation.Controllers
{
    [Route("api/users")]
    [Authorize]
    public class UsersController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;

        public UsersController(ApplicationDbContext context, IMapper mapper, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("createStudent")]
        [ProducesResponseType(typeof(int), 200)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> CreateStudentAsync(CreateStudentDto request)
        {
            if (await IsUserIdExists(request.Id))
                return Conflict("Id already exists.");

            var user = new User
            {
                Id = request.Id,
                Name = request.Name,
                HashedPassword = _passwordHasher.HashPassword(null!, request.Password),
                Role = Roles.Student,
                StudentDetails = new StudentDetails
                {
                    GPA = request.GPA
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user.Id);
        }

        [HttpPost("createStudents")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> CreateStudentsAsync(IFormFile file)
        {
            if (!IsExcelFile(file))
                return BadRequest("Only excel files with xlsx extensions are allowed.");

            var students = new List<User>();

            using (var stream = new MemoryStream()) {

                await file.CopyToAsync(stream);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(stream))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (workSheet == null || workSheet.Dimension.Rows == 0)
                        return BadRequest("Excel file is empty");

                    try
                    {
                        for (var row = 2; row <= workSheet.Dimension.Rows; ++row)
                        {
                            students.Add(new User
                            {
                                Id = int.Parse(workSheet.Cells[row, 1].Value.ToString()),
                                Name = workSheet.Cells[row, 2].Value.ToString(),
                                HashedPassword = _passwordHasher.HashPassword(null!, workSheet.Cells[row, 2].Value.ToString()),
                                Role = Roles.Student,
                                StudentDetails = new StudentDetails
                                {
                                    GPA = double.Parse(workSheet.Cells[row, 4].Value.ToString())
                                }
                            });
                        }
                    }
                    catch
                    {
                        return BadRequest("Excel file is can't be mapped.");
                    }
                }
            }

            _context.Users.AddRange(students);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static bool IsExcelFile(IFormFile file)
        {
            return file.FileName.EndsWith(".xlsx");
        }

        [HttpPut("updateStudent")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> UpdateStudentAsync(UpdateStudentDto request)
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u=>u.Id == request.Id && u.Role == Roles.Student);

            if (student == null)
                return NotFound();

            student.Name = request.Name;
            student.StudentDetails.GPA = request.GPA;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("createDoctor")]
        [ProducesResponseType(typeof(int), 200)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> CreateDoctorAsync(CreateDoctorDto request)
        {
            if (await IsUserIdExists(request.Id))
                return Conflict("Id already exists.");

            var user = new User
            {
                Id = request.Id,
                Name = request.Name,
                HashedPassword = _passwordHasher.HashPassword(null!, request.Password),
                Role = Roles.Doctor,
                DoctorDetails = new DoctorDetails
                {
                    MaxProjects = request.MaxProjects
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user.Id);
        }

        [HttpPost("createDoctors")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> CreateDoctorsAsync(IFormFile file)
        {
            if (!IsExcelFile(file))
                return BadRequest("Only excel files with xlsx extensions are allowed.");

            var students = new List<User>();

            using (var stream = new MemoryStream())
            {

                await file.CopyToAsync(stream);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(stream))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (workSheet == null || workSheet.Dimension.Rows == 0)
                        return BadRequest("Excel file is empty");

                    try
                    {
                        for (var row = 2; row <= workSheet.Dimension.Rows; ++row)
                        {
                            students.Add(new User
                            {
                                Id = int.Parse(workSheet.Cells[row, 1].Value.ToString()),
                                Name = workSheet.Cells[row, 2].Value.ToString(),
                                HashedPassword = _passwordHasher.HashPassword(null!, workSheet.Cells[row, 2].Value.ToString()),
                                Role = Roles.Doctor,
                                DoctorDetails = new DoctorDetails
                                {
                                    MaxProjects = int.Parse(workSheet.Cells[row, 4].Value.ToString())
                                }
                            });
                        }
                    }
                    catch
                    {
                        return BadRequest("Excel file is can't be mapped.");
                    }
                }
            }

            _context.Users.AddRange(students);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("updateDoctor")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> UpdateDoctorAsync(UpdateDoctorDto request)
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id && u.Role == Roles.Doctor);

            if (student == null)
                return NotFound();

            student.Name = request.Name;
            student.DoctorDetails.MaxProjects = request.MaxProjects;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("createAdmin")]
        [ProducesResponseType(typeof(int), 200)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> CreateAdminAsync(CreateAdminDto request)
        {
            if (await IsUserIdExists(request.Id))
                return Conflict("Id already exists.");

            var user = new User
            {
                Id = request.Id,
                Name = request.Name,
                HashedPassword = _passwordHasher.HashPassword(null!, request.Password),
                Role = Roles.Admin
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user.Id);
        }

        [HttpPut("updateAdmin")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> UpdateAdminAsync(UpdateAdminDto request)
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id && u.Role == Roles.Admin);

            if (student == null)
                return NotFound();

            student.Name = request.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("id")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            var projects = await _context.Projects.Where(t => t.DoctorId == id ||
                t.AssistantDoctorId == id ||
                t.TeamLeaderId == id)
                .ToListAsync();

            var requests = await _context.ProjectIdeaRequests.Where(r => r.DoctorId == id ||
                r.AssistantDoctorId == id ||
                r.TeamLeaderId == id)
                .ToListAsync();

            requests.ForEach(r => FileHelpers.Remove(r.FileUrl));

            _context.Projects.RemoveRange(projects);
            _context.ProjectIdeaRequests.RemoveRange(requests);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("resetPassword")]
        [ProducesResponseType(204)]
        [HasRole(Roles.Admin)]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _context.Users.FindAsync(request.UserId);

            if (user == null)
                return NotFound();

            user.HashedPassword = _passwordHasher.HashPassword(null!, request.Password);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("id")]
        [ProducesResponseType(typeof(UserDto), 200)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           var response = await _context.Users
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(u => u.Id == id);

            if(response == null)
                return NotFound();

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        public async Task<IActionResult> GetAllAsync(string role = null)
        {
            var query = _context.Users.AsQueryable();

            if (role != null && Enum.IsDefined(typeof(Roles), role))
                query = query.Where(u => u.Role == Enum.Parse<Roles>(role));
            
            return Ok(await query
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .ToListAsync());
        }

        [HttpPost("isIdExists")]
        [ProducesResponseType(typeof(bool), 200)]
        [AllowAnonymous]
        public async Task<IActionResult> IsIdExistsAsync(IsIdExistsDto request)
        {
            return Ok(await IsUserIdExists(request.Id));
        }

        private async Task<bool> IsUserIdExists(int id)
        => await _context.Users.AnyAsync(u => u.Id == id);
    }
}
