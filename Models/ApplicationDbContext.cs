

namespace FirstWebApi.Models
{
	public class ApplicationDbContext : DbContext
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {
            
        }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new GenreConfig());
		}

		public virtual DbSet<Genre> Genres { get;set; }
	}
}
