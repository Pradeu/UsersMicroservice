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
        private readonly ILogger _logger;

        public UserController(
            MainContext context, IMapper mapper, JwtService jwtService, ILogger<UserController> logger)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DtoUser>>> GetUsersAsync()
        {
                var dbUsers = await _context.Users.Include(g => g.UserGames).ToListAsync();
                return _mapper.Map<List<DtoUser>>(dbUsers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DtoUser>> GetUsersByIdAsync(int id)
        {
            var dbUser = await _context.Users.Include(g => g.UserGames).FirstOrDefaultAsync(g => g.Id == id);
            Console.WriteLine(dbUser);
            if (dbUser == null)
            {
                _logger.LogWarning($"Ошибка: пользователь с id {id} не найден в системе");
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
                _logger.LogInformation($"Пользователь {dbUser.Name} зарегистрировался в {DateTimeOffset.UtcNow.}");
                dtoUser = _mapper.Map<DtoUser>(dbUser);
                return dtoUser;
            }
            catch (Exception)
            {
                _logger.LogError($"Попытка регистрации нового пользователя {dtoUser.Name} на существующую почту {dtoUser.Email}");
                return BadRequest(new {message = "Не удалось зарегистрировать пользователя в системе"});
            }
            
        }


        [HttpPost("login")]

        public async Task<ActionResult<DtoLogin>> LoginUserAsync(
             [FromBody] DtoLogin dtoLogin)
        {
            var user = await _context.Users.Include(g => g.UserGames).FirstOrDefaultAsync(g => g.Email == dtoLogin.Email);

            if (user == null)
            {
                _logger.LogWarning($"Ошибка: пользователя с введенными данными: {dtoLogin.Email}, {dtoLogin.Password} нет в системе");
                return BadRequest(new { message = "Введенные данные неверны" });
            }

            if (!BCrypt.Net.BCrypt.Verify(dtoLogin.Password, user.Password))
            {
                _logger.LogWarning($"Ошибка: неверно введен пароль от учетной записи {dtoLogin.Email}");
                return BadRequest(new { message = "Введенные данные неверны" });
            }

            var jwt = _jwtService.Generate(user.Id);

            if (jwt == null)
            {
                _logger.LogError($"Ошибка: не удалось сгенерировать jwt токен для пользователя {dtoLogin.Email}");
                return NoContent();
            }

            Response.Cookies.Append("jwt", jwt, new CookieOptions
            {
                HttpOnly = true
            });

            return Ok(user);
        }

        [HttpPost("updateuser")]
        public async Task<ActionResult<DtoUser>> UpdateUserName()
        {
            return BadRequest(new { message = "new"});
        }

        [HttpGet("jwtuser")]
        public async Task<ActionResult<DtoUser>> GetJwtUserAsync()
        {
            try
            {
                var jwt = Request.Cookies["jwt"];
                var token = _jwtService.Verify(jwt);
                int userId = int.Parse(token.Issuer);
                var user = await _context.Users.Include(g => g.UserGames).FirstOrDefaultAsync(g => g.Id == userId);

                return _mapper.Map<DtoUser>(user);
            }
            catch(Exception)
            {
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

        [HttpPost("addgame")]
        public async Task<ActionResult<DtoUserGame>> PostUserGameList(
            [FromBody] DtoUserGame dtoUserGame
            )
        {
            var gameList = new DtoUserGame
            {
                UserId = dtoUserGame.UserId,
                GameId = dtoUserGame.GameId,
            };
            var dbUserGame = _mapper.Map<DbUserGame>(gameList);

            _context.UserGames.Add(dbUserGame);
            await _context.SaveChangesAsync();

            dtoUserGame = _mapper.Map<DtoUserGame>(dbUserGame);

            return dtoUserGame;
        }

        [HttpPost("deletegame")]
        public async Task<ActionResult<DtoUserGame>> DeleteUserGameList(int UserId, int GameId)
        {
            var data = await _context.UserGames.FirstOrDefaultAsync(game => game.UserId == UserId && game.GameId == GameId);

            if (data == null)
            {
                return NotFound();
            }

            _context.UserGames.Remove(data);
            await _context.SaveChangesAsync();

            return Ok("Игра успешно удалена из списка");
        }

    }
}
