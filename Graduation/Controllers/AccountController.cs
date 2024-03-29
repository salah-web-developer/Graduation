using AutoMapper;
using AutoMapper.QueryableExtensions;
using Graduation.Data;
using Graduation.Dtos.Account;
using Graduation.Dtos.Users;
using Graduation.Entities;
using Graduation.Extensions;
using Graduation.Services.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation.Controllers
{
    [Route("api/account")]
    [Authorize]
    public class AccountController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IJwtService jwtService, IMapper mapper)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(AuthResult), 200)]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate(AuthenticateDto request)
        {
            var user = await _context.Users.FindAsync(request.Id);

            if (user == null || _passwordHasher.VerifyHashedPassword(null!, user.HashedPassword, request.Password) == 0)
                return Unauthorized("Id or password isn't correct");

            return Ok(new AuthResult
            {
                Id = user.Id,
                Name = user.Name,
                Role = user.Role.ToString(),
                Token = _jwtService.GenerateJwtToken(user)
            });
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserDto), 200)]
        public async Task<IActionResult> Details()
        {
            return Ok(await _context.Users
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(u=>u.Id == User.Id()));
        }
    }
}
