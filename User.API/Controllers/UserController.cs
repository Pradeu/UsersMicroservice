using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.API.Helpers;
using User.API.Models;
using User.DB;
using User.DB.Entities;

namespace User.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MainContext _context;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;

        public UserController(
            MainContext context, IMapper mapper, JwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DtoUser>>> GetUsersAsync()
        {
                var dbUsers = await _context.Users.ToListAsync();
                Console.WriteLine("Количество пользователей, найденных в базе данных: " + dbUsers.Count);
                return _mapper.Map<List<DtoUser>>(dbUsers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DtoUser>> GetUsersByIdAsync(int id)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(g => g.Id == id);
            Console.WriteLine(dbUser);
            if (dbUser == null)
            {
                Console.WriteLine("Ошибка: пользователь с id " + id + " не найден");
                return NotFound();
            }

            return _mapper.Map<DtoUser>(dbUser);


        }

        [HttpPost("register")]

        public async Task<ActionResult<DtoUser>> PostUserAsync(
            [FromBody] DtoUser dtoUser
        )
        {
            try {
                var user = new DtoUser
                {
                    Name = dtoUser.Name,
                    Email = dtoUser.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(dtoUser.Password)
                };
                var dbUser = _mapper.Map<DbUser>(user);

                _context.Users.Add(dbUser);
                await _context.SaveChangesAsync();

                dtoUser = _mapper.Map<DtoUser>(dbUser);

                return dtoUser;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Не удалось зарегистрировать пользователя в системе");
                return BadRequest(new {message = "Не удалось зарегистрировать пользователя в системе"});
            }
            
        }


        [HttpPost("login")]

        public async Task<ActionResult<DtoLogin>> LoginUserAsync(
             [FromBody] DtoLogin dtoLogin)
        {
            var user = await _context.Users.FirstOrDefaultAsync(g => g.Email == dtoLogin.Email);

            if (user == null) {
                Console.WriteLine("Ошибка: пользователя с такими данными нет в системе");
                return BadRequest(new { message = "Данные неверны" }); 
            } 

            if (!BCrypt.Net.BCrypt.Verify(dtoLogin.Password, user.Password))
            {
                Console.WriteLine("Ошибка: неверно введен пароль от учетной записи");
                return BadRequest(new { message = "Данные неверны" });
            }

            var jwt = _jwtService.Generate(user.Id);

            Response.Cookies.Append("jwt", jwt, new CookieOptions
            {
                HttpOnly = true
            });

            return Ok(user);
        }

        [HttpGet("jwtuser")]
        public async Task<ActionResult> GetJwtUserAsync()
        {
            try
            {
                var jwt = Request.Cookies["jwt"];
                var token = _jwtService.Verify(jwt);
                int userId = int.Parse(token.Issuer);
                var user = await _context.Users.FirstOrDefaultAsync(g => g.Id == userId);

                return Ok(user);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Ошибка: Вы не авторизовались в системе");
                return Unauthorized();
            }
        }
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new
            {
                message = "Вы вышли из учетной записи"
            });
        }
    }
}
