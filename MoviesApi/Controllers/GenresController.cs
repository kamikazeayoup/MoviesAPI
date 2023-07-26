using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public GenresController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var generes = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            return Ok(generes);

        }
        [HttpPost]
        public async Task<IActionResult> CreateNewGenAsync(CreateGenreDTO dto )
        {
            var genre = new Genre() { Name = dto.Name};
            await _context.AddAsync(genre);
            _context.SaveChanges();
            return Ok(genre);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult>EditAsync(int id , [FromBody] CreateGenreDTO dto)
        {
            var genre = await _context.Genres.SingleOrDefaultAsync(I => I.Id == id);
            if (genre == null)
            {
                return NotFound($"The Id {id} that you've entered is not Stored in Database ");
            }
            genre.Name = dto.Name;
            _context.SaveChanges();
            return Ok(genre);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult>DeletebyId(int id)
        {
            var genre = await _context.Genres.SingleOrDefaultAsync(I => I.Id==id);
            if (genre == null)
                return NotFound("there's no id in this id");
            _context.Remove(genre);
            _context.SaveChanges();
            return Ok("Deleted");

        
        }
    }
}
