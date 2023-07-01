using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.DTOs;
using MoviesApi.Models;
using MoviesApi.Models.Errors;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

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
			try
			{
				return new OkObjectResult(new CustomOkResponse<List<Genre>>()
				{
					Message = "Get All Genres Successfully .",
					StatusCode = 200,
					Data = await _context.Genres.ToListAsync()
				});
			}
			catch
			{
				return BadRequest();
			}
		}

		[HttpPost]
		public async Task<IActionResult> CreateGenreAsync(GenreRequestDto genre)
		{
			if (genre.Name.IsNullOrEmpty())
			{

				return new NotFoundObjectResult(new CustomOkResponse<object>()
				{
					Status = false,
					StatusCode = 404,
					Message = "Genre Name Should be specified !!",
				});
			}
			else
			{
				var g = new Genre { Name = genre.Name };
				await _context.AddAsync(g);
				_context.SaveChanges();

				// Only one message
				//return Content(HttpStatusCode.Created.ToString(), "Created Genre Succfully");

				return new ObjectResult(
					new CustomOkResponse<Genre>()
					{ StatusCode = 201, Data = g, Message = "Created Genre Succefully !!" })
				{ StatusCode = StatusCodes.Status201Created };


				//return new OkObjectResult(new CustomOkResponse<Genre>() { Data = g , StatusCode = 201});
			}
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAsync(int id, GenreRequestDto dto)
		{
			if (dto == null)
			{
				return BadRequest("The Genre Name should be defined !!");
			}

			var genre = await _context.Genres.FirstOrDefaultAsync(g => g.ID == id);
			if (genre is null)
			{
				return NotFound(new NotFoundError($"Cannot found Genre with ID : {id}"));
			}

			genre.Name = dto.Name;
			_context.SaveChanges();


			return Ok(genre);
		}


		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAsync(int id)
		{
			var genre = await _context.Genres.FirstOrDefaultAsync(g => g.ID == id);
			if (genre is null)
			{
				return NotFound(new NotFoundError($"Cannot found Genre with ID : {id}"));
			}

			_context.Remove(genre);
			_context.SaveChanges();

			return Ok(genre);
		}
	}
}
