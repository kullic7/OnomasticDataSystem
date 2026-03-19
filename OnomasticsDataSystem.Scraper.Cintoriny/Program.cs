using OnomasticsDataSystem.Scraper.Cintoriny;

class Program
{
	static async Task Main()
	{
		var scraperCintoriny = new Sc();
		await scraperCintoriny.RunAsync();
		//Console.WriteLine($"\nZískaných {persons.Count} mien.");
	}
}