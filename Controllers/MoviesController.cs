

namespace MoviesApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MoviesController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly string[] _allowedImageExtensions = { ".jpg", ".png" };
		private readonly long _maxPosterLengthInByte = 1048576;

		public MoviesController(ApplicationDbContext context)
		{
			_context = context;
		}


		[HttpGet]
		public async Task<IActionResult> GetAllAsync()
		{
			var movies = await _context
				.Movies
				.Include(M => M.Genre)
				.AsNoTracking()
				.Select(M => new MovieResponseDto()
				{
					ID = M.ID,
					Title = M.Title,
					Genre = new GenreResponseDto()
					{
						Id = M.Genre.ID,
						Name = M.Genre.Name
					}
				})
				.ToListAsync();
			return SuccessObjectResult<List<MovieResponseDto>>(movies, (int)HttpStatusCode.OK);
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetMovieByIdAsync(int id)
		{
			var movie = await _context
				.Movies
				.Include(M => M.Genre)
				.Select(M => new MovieResponseDto()
				{
					ID = M.ID,
					Title = M.Title,
					Genre = new GenreResponseDto()
					{
						Id = M.Genre.ID,
						Name = M.Genre.Name
					},
					StoryLine = M.StoryLine,
					Rate = M.Rate,
					Year = M.Year
				})
				.FirstOrDefaultAsync(M => M.ID == id);
			//var movieDto = new MovieResponseDto();
			if (movie is null)
			{
				return new NotFoundObjectResult(
					new CustomResponse<object>()
					{
						//Errors = new List<string> { "The provided Movide ID was not exists" },
						Message = "The provided Movie ID was not exists",
						StatusCode = (int)HttpStatusCode.NotFound,
						Status = false,

					}

				);
			}

			return SuccessObjectResult<MovieResponseDto>(movie, (int)HttpStatusCode.OK);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromForm] MovieRequestDto dto)
		{
			var errors = new List<string>();
			await CreateMovieErrorsHandler(dto, errors);

			if (errors.Count > 0)
			{
				return HandleBadRequestError(errors);
			}

			using var dataStream = new MemoryStream();
			await dto.Poster!.CopyToAsync(dataStream);
			var movie = new Movie
			{
				GenreID = dto.GenreID!.Value,
				Poster = dataStream.ToArray(),
				Rate = dto.Rate!.Value,
				StoryLine = dto.StoryLine!,
				Title = dto.Title,
				Year = dto.Year!.Value,
			};
			_context.Add(movie);
			await _context.SaveChangesAsync();

			return SuccessObjectResult<Movie>(movie, (int)HttpStatusCode.Created);
		}

		private static ObjectResult SuccessObjectResult<T>(T data, int statusCode)
		{
			return new ObjectResult(new CustomResponse<T>()
			{
				StatusCode = statusCode,
				Data = data,
				Message = "Success",
			})
			{ StatusCode = statusCode };
		}

		private async Task CreateMovieErrorsHandler(MovieRequestDto dto, List<string> errors)
		{
			if (dto is null)
			{
				errors.Add("Please Provide you Movie info");
				return;
			}
			var isValidGenre = await _context.Genres.AnyAsync(G => G.ID == dto.GenreID);
			if (dto.Poster is null)
			{
				errors.Add(HandleErrorMessage("Poster"));
			}
			else if (dto.Poster.Length > _maxPosterLengthInByte)
			{
				errors.Add("The Poster size is too big , it shouldn't exceed 1MB");
			}
			else if (!_allowedImageExtensions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
			{
				errors.Add("You Poster image should be .png / .jpg Only");
			}

			if (dto.Title.IsNullOrEmpty())
			{
				errors.Add(HandleErrorMessage("Title"));
			}

			if (dto.Rate is null)
			{
				errors.Add(HandleErrorMessage("Rate"));
			}
			else if (dto.Rate.Value < 0 || dto.Rate.Value > 10)
			{
				errors.Add("Invalid Rate , the Rate should be between 0 - 10");
			}

			if (dto.GenreID is null)
			{
				errors.Add("You should provide GenreId for the Movie");
			}
			else if (dto.GenreID <= 0)
			{
				errors.Add("The Genre ID should be more than 0 ");
			}
			else if (!isValidGenre)
			{
				errors.Add($"The Provided Genre ID : {dto.GenreID} doesn't exists !!");
			}

		}

		private string HandleErrorMessage(string name)
		{
			return $"You should send {name}";
		}

		private static ObjectResult HandleBadRequestError(List<string> errors)
		{
			return new BadRequestObjectResult(new CustomResponse<object>
			{
				Message = "Failure",
				Status = false,
				StatusCode = (int)HttpStatusCode.BadRequest,
				Errors = errors
			});
		}
	}


}
