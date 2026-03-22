using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OnomasticDataSystem.DataImport.Cleaning;
using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Interfaces;

namespace OnomasticDataSystem.DataImport.Services
{
	public class ImportPerson
	{
		private readonly IPersonRepository _personRepository;
		private readonly ISourceRepository _sourceRepository;
		private readonly IConfiguration _configuration;
		public ImportPerson(
			IConfiguration configuration, IPersonRepository personRepository, ISourceRepository sourceRepository)
		{
			_configuration = configuration;
			_personRepository = personRepository;
			_sourceRepository = sourceRepository;
		}

		public async Task ImportAllAsync()
		{
			//upravit
			var basePath = AppContext.BaseDirectory;
			var cintorinyPath = Path.Combine(basePath, _configuration["ImportPaths:Cintoriny"]);
			var absolventiPath = Path.Combine(basePath, _configuration["ImportPaths:AbsolventiUniba"]);
			var dlzniciSocPath = Path.Combine(basePath, _configuration["ImportPaths:DlzniciSoc"]);
			var dlzniciUnionPath = Path.Combine(basePath, _configuration["ImportPaths:DlzniciUnion"]);
			var dlzniciVszpPath = Path.Combine(basePath, _configuration["ImportPaths:DlzniciVszp"]);
			var regPravOsobPath = Path.Combine(basePath, _configuration["ImportPaths:RegPravOsob"]);
			await ImportFromSourceAsync(
				"Register pravnickych osôb",
				new RegPravOsobCleaner().Clean(regPravOsobPath));

			await ImportFromSourceAsync(
				"Cintoriny",
				new CintorinyCleaner().Clean(cintorinyPath));

			await ImportFromSourceAsync(
				"Absolventi Uniba",
				new AbsolventiUnibaCleaner().Clean(absolventiPath));

			await ImportFromSourceAsync(
				"Dlznici socialna poistovna",
				new DlzniciSocPoistovnaCleaner().Clean(dlzniciSocPath));

			await ImportFromSourceAsync(
				"Dlznici union poistovna",
				new DlzniciUnionPoistovnaCleaner().Clean(dlzniciUnionPath));

			await ImportFromSourceAsync(
				"Dlznici VSZP poistovna",
				new DlzniciVszpPoistovnaCleaner().Clean(dlzniciVszpPath));


		}

		private async Task ImportFromSourceAsync(string sourceName, List<Person> people)
		{
			Console.WriteLine($"{sourceName} -> {people.Count} records");
			var source = await _sourceRepository.GetByNameAsync(sourceName);

			const int batchSize = 10000;

			for (int i = 0; i < people.Count; i += batchSize)
			{
				var batch = people.Skip(i).Take(batchSize).ToList();

				foreach (var person in batch)
				{
					person.SourceId = source.Id;
					person.CreatedAt = DateTime.UtcNow;
				}

				try
				{
					await _personRepository.InsertIgnoreConflictsAsync(batch);
				}
				catch (DbUpdateException ex)
				{
					Console.WriteLine(ex.InnerException?.Message);
				}
				_personRepository.ClearTracker();
			}
		}
	}
}
