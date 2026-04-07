using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OnomasticDataSystem.DataImport.Cleaning;
using OnomasticDataSystem.DataImport.Services;
using OnomasticsDataSystem.Common;
using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Interfaces;
using OnomasticsDataSystem.Infrastructure.Data;
using OnomasticsDataSystem.Infrastructure.Repositories;

var configuration = new ConfigurationBuilder()
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json")
	.Build();

var services = new ServiceCollection();
services.AddDbContextFactory<ApplicationDbContext>(options =>
	options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
services.AddScoped<ISourceRepository, SourceRepository>();
services.AddScoped<SourceSeeder>();

services.AddScoped<IPersonRepository, PersonRepository>();
services.AddScoped<ImportPerson>();

services.AddSingleton<IConfiguration>(configuration);

var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();

var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
using var db = factory.CreateDbContext();
db.Database.Migrate();

//nahrat mapu mien
var path = configuration["ImportPaths:NameVariants"];

Helper.LoadNamesFromCsv(
	Path.Combine(AppContext.BaseDirectory, path)
);
Console.WriteLine("Mapa mien nahrata." + Helper.NameMap.Count);

//vytvor a spusti seeder
var seeder = scope.ServiceProvider.GetRequiredService<SourceSeeder>();
await seeder.SeedAsync();
var importer = scope.ServiceProvider.GetRequiredService<ImportPerson>();
await importer.ImportAllAsync();