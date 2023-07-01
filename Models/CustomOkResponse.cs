namespace MoviesApi.Models
{
	public class CustomOkResponse<T> 
	{
		public bool Status { get; set; } = true;	
		public int StatusCode { get; set; }

		public string Message { get; set; }

		public T Data { get; set; }
	}
}
