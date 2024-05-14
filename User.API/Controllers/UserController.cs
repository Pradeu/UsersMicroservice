using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public UserController(
            MainContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DtoUser>>> GetUsersAsync()
        {
            var dbUsers = await _context.Users.ToListAsync();
            return _mapper.Map<List<DtoUser>>(dbUsers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DtoUser>> GetUsersByIdAsync(int id)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(g => g.Id == id);
            Console.WriteLine(dbUser);
            if (dbUser == null)
            {
                return NotFound();
            }

            return _mapper.Map<DtoUser>(dbUser);


        }

        [HttpPost]

        public async Task<ActionResult<DtoUser>> PostUserAsync(
            [FromBody] DtoUser dtoUser
        )
        {
            var dbUser = _mapper.Map<DbUser>(dtoUser);

            _context.Users.Add(dbUser);
            await _context.SaveChangesAsync();

            dtoUser = _mapper.Map<DtoUser>(dbUser);

            return dtoUser;
        }
    }
}
