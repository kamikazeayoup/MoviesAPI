using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Models;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private List<string> _allowedExtentions = new List<string>() { ".jpg" , ".png"};
        private long _MaxAllowedSize = 1048576;



        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
            
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _context.
                Movies.
                Include(m=>m.Genre)
                .Select(m => new MovieDetail()
                {
                    Id = m.Id,
                    GenreId = m.GenreId,
                    Title = m.Title,
                    Year = m.Year,
                    Storeline = m.Storeline,
                    Rate = m.Rate,
                    Poster = m.Poster,
                    GenreName = m.Genre.Name

                })
                .OrderByDescending(g => g.Rate)
                .ToListAsync();
            return Ok(movies);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetmovieIDAsync(int id)
        {
            var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(g => g.Id == id);
            if(movie == null )
                return NotFound("No Movie in this id");

            var dto = new MovieDetail
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                Title = movie.Title,
                Year = movie.Year,
                Storeline = movie.Storeline,
                Rate = movie.Rate,
                Poster = movie.Poster,
                GenreName = movie.Genre?.Name

            };

            return Ok(dto);
        }
        [HttpGet("GetByGenreId")]
        public async Task<IActionResult>GetbyGenereId(byte genereId)
        {

            var movies = await _context.
             Movies.
             Include(m => m.Genre)
             .Where(I => I.GenreId == genereId)
             .Select(m => new MovieDetail()
             {
                 Id = m.Id,
                 GenreId = m.GenreId,
                 Title = m.Title,
                 Year = m.Year,
                 Storeline = m.Storeline,
                 Rate = m.Rate,
                 Poster = m.Poster,
                 GenreName = m.Genre.Name

             })
             .OrderByDescending(g => g.Rate)
             .ToListAsync();

            if (movies == null)
                return NotFound("No Movies with this category");
            return Ok(movies);
        }



        [HttpPost]
        public async Task<IActionResult> CreateMovieAsync([FromForm]MovieDto dto)
        {
            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenre)
                return BadRequest("No Genres available for this movie");
            if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only jpg and png are allowed");
            if (dto.Poster.Length > _MaxAllowedSize)
                return BadRequest("size exceed");
            
            using var datastream = new MemoryStream();
            await dto.Poster.CopyToAsync(datastream);
            var movie = new Movie(){
                GenreId = dto.GenreId,
                Title = dto.Title,
                Storeline = dto.Storeline,
                Rate = dto.Rate,
                Poster = datastream.ToArray(),
                Year = dto.Year,
            };
            await _context.AddAsync(movie);
            _context.SaveChanges();
            return Ok(movie);
            
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            var movie = await _context.Movies.SingleOrDefaultAsync(x => x.Id == id);
            if (movie == null)
                return NotFound("NO ID Contains");
            _context.Remove(movie);
            _context.SaveChanges();
            return Ok(movie);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditByIDAsync(int id , [FromForm] MovieDto dto )
        {
            var movie = await _context.Movies.SingleOrDefaultAsync(x => x.Id == id);
            if (movie == null)
                return NotFound("NO ID Contains");

            if (dto.Poster != null)
            {
                if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only jpg and png are allowed");
                if (dto.Poster.Length > _MaxAllowedSize)
                    return BadRequest("size exceed");

                using var datastream = new MemoryStream();
                 await dto.Poster.CopyToAsync(datastream);
                movie.Poster = datastream.ToArray();



            }
            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenre)
                return BadRequest("No Genres available for this movie");

            movie.Title = dto.Title;
            movie.Storeline = dto.Storeline;
            movie.Rate = dto.Rate;
            movie.Year = dto.Year;
            movie.GenreId = dto.GenreId;

            _context.SaveChanges();
            return Ok(movie);

        }
    }
}
