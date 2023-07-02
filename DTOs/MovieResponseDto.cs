namespace MoviesApi.DTOs
{
	public record MovieResponseDto
	{
        public  int Id { get; set; }
        public string   Name { get; set; }

        public GenreResponseDto Genre { get; set; }
    }
}
