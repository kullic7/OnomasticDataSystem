using Microsoft.Playwright;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
	public static async Task Main()
	{
		int maxPages = 4905; // nastav podľa potreby
		string outputFile = "dlznici.csv";

		// CSV header
		var sb = new StringBuilder();
		sb.AppendLine("meno;priezvisko;mesto");

		using var playwright = await Playwright.CreateAsync();
		await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = true
		});

		var page = await browser.NewPageAsync();

		for (int p = 1; p <= maxPages; p++)
		{
			string url =
				$"https://www.vszp.sk/platitelia/platenie-poistneho/zoznam-dlznikov.html?docid=227&nazov=&typ=0&page={p}&proceed=true#vyhl";

			Console.WriteLine($"Načítavam stránku {p}...");

			await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			var rows = await page.QuerySelectorAllAsync("table.tabulkaStandard tbody tr");
			if (rows.Count == 0)
			{
				Console.WriteLine("Žiadne riadky – končím.");
				break;
			}

			foreach (var row in rows)
			{
				var cells = await row.QuerySelectorAllAsync("td");
				if (cells.Count < 2) continue;

				string fullName = (await cells[0].InnerTextAsync()).Trim();
				string city = (await cells[1].InnerTextAsync()).Trim();

				// Rozdelenie mena a priezviska
				string[] parts = Regex.Split(fullName, @"\s+");
				if (parts.Length == 0) continue;

				string priezvisko = parts[0];
				string meno = string.Join(" ", parts.Skip(1));

				// zapíš do CSV
				sb.AppendLine($"{meno};{priezvisko};{city}");
			}
		}

		// uloženie CSV
		await File.WriteAllTextAsync(outputFile, sb.ToString(), Encoding.UTF8);

		Console.WriteLine($"Hotovo! Výsledky uložené v súbore: {outputFile}");
	}
}
