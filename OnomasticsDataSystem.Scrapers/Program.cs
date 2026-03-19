using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Interfaces;
using OnomasticsDataSystem.Infrastructure.Data;
using OnomasticsDataSystem.Infrastructure.Repositories;

class Program
{
	static async Task Main()
	{
		//Načíta konfiguráciu (connection string)
		//var configuration = new ConfigurationBuilder()
		//	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		//	.Build();

		////Nastaví služby (DI)
		//var services = new ServiceCollection();

		//services.AddDbContext<ApplicationDbContext>(options =>
		//	options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

		//services.AddScoped<IPersonRepository, PersonRepository>();

		////Vytvorí ServiceProvider
		//using var provider = services.BuildServiceProvider();

		////Získa repozitár
		//var repo = provider.GetRequiredService<IPersonRepository>();

		
		//foreach (var person in persons) {
		//	await repo.AddAsync(person);
		//}
		
	}
}

