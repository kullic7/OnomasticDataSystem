using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnomasticsDataSystem.Infrastructure.Data
{
	public class ApplicationDbContextFactory
		: IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

			optionsBuilder.UseNpgsql(
				"Host=localhost;Port=5432;Database=OnomasticsDatabase;Username=rado;Password=mypassword");

			return new ApplicationDbContext(optionsBuilder.Options);
		}
	}
}
