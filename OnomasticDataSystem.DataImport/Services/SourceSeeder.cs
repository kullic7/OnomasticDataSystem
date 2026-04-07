using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Interfaces;

namespace OnomasticDataSystem.DataImport.Services
{
	public class SourceSeeder
	{
		private readonly ISourceRepository _repository;

		public SourceSeeder(ISourceRepository repository)
		{
			_repository = repository;
		}

		public async Task SeedAsync()
		{
			await SeedSourceAsync("Absolventi Uniba", "https://absolventi.uniba.sk/sk");
			await SeedSourceAsync("Cintoríny", "https://www.cintoriny.sk/src/index.php");
			await SeedSourceAsync("Dlžníci Sociálna poisťovna", "https://www.socpoist.sk/nastroje-sluzby/zoznam-dlznikov");
			await SeedSourceAsync("Dlžníci Union poisťovna", "https://portal.unionzp.sk/pub/dlznici");
			await SeedSourceAsync("Dlžníci VŠZP poisťovna", "https://www.vszp.sk/platitelia/platenie-poistneho/zoznam-dlznikov.html");
			await SeedSourceAsync("Register právnických osôb", "https://frkqbrydxwdp.compat.objectstorage.eu-frankfurt-1.oraclecloud.com/susr-rpo/");
		}

		private async Task SeedSourceAsync(string name, string url)
		{
			var sources = await _repository.GetAllAsync();
			var exists = sources.Any(s => s.Name == name);

			if (!exists)
			{
				var source = new Source
				{
					Name = name,
					Url = url,
					CreatedAt = DateTime.UtcNow
				};

				await _repository.AddAsync(source);
				await _repository.SaveChangesAsync();
			}
		}
	}
}