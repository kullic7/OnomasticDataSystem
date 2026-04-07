using Microsoft.EntityFrameworkCore;
using OnomasticsDataSystem.Core.Entity;

namespace OnomasticsDataSystem.Infrastructure.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		// tabulky
		public DbSet<Person> People { get; set; } = null!;
		public DbSet<Source> Sources { get; set; } = null!;

		//najde vsetky triedy ktore implementuju IEntityTypeConfiguration a aplikuje ich konfiguraciu
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		}
	}
}
