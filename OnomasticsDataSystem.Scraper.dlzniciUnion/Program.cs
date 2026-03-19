using Microsoft.Playwright;
using System.Text;

class Program
{
	static async Task Main()
	{
		Console.WriteLine("▶ Spúšťam Playwright...");

		using var playwright = await Playwright.CreateAsync();
		await using var browser = await playwright.Chromium.LaunchAsync(new()
		{
			Headless = true
		});

		var page = await browser.NewPageAsync();

		Console.WriteLine("▶ Otváram stránku...");
		await page.GotoAsync("https://portal.unionzp.sk/pub/dlznici", new()
		{
			WaitUntil = WaitUntilState.NetworkIdle
		});

		await page.WaitForSelectorAsync("tbody.v-data-table__tbody");
		Console.WriteLine("Tabuľka načítaná");

		using var writer = new StreamWriter("dlznici.csv", false, Encoding.UTF8);
		writer.WriteLine("FirstName;LastName;Address");

		int pageIndex = 1;
		int totalRows = 0;

		while (true)
		{
			Console.WriteLine($"\nSpracúvam stránku {pageIndex}");

			var rows = await page.QuerySelectorAllAsync(
				"tbody.v-data-table__tbody tr.v-data-table__tr"
			);

			Console.WriteLine($"  Nájdených riadkov: {rows.Count}");

			foreach (var row in rows)
			{
				var cells = await row.QuerySelectorAllAsync(
					"td.v-data-table__td"
				);

				if (cells.Count < 5)
				{
					Console.WriteLine("  Preskakujem riadok – málo stĺpcov");
					continue;
				}

				var fullName = (await cells[0].InnerTextAsync()).Trim();
				var rawAddress = (await cells[4].InnerTextAsync()).Trim();

				// nájdi PSČ (5 číslic)
				var match = System.Text.RegularExpressions.Regex.Match(rawAddress, @"\d{5}");

				string city = "";

				if (match.Success)
				{
					var indexAfterPsc = match.Index + match.Length;
					city = rawAddress.Substring(indexAfterPsc).Trim();
				}

				var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

				var firstName = nameParts.Length >= 1 ? nameParts[0] : "";
				var lastName = nameParts.Length >= 2 ? nameParts[1] : "";

				Console.WriteLine(
					$"    ✔ {firstName} {lastName} | {city}"
				);

				writer.WriteLine(
					$"{Csv(firstName)};{Csv(lastName)};{Csv(city)}"
				);

				totalRows++;
			}

			var nextButton = await page.QuerySelectorAsync(
				"button[aria-label='Ďalšia stránka']"
			);

			if (nextButton == null)
			{
				Console.WriteLine("Tlačidlo 'Ďalšia stránka' nenájdené – končím");
				break;
			}

			var disabled = await nextButton.GetAttributeAsync("aria-disabled");
			if (disabled == "true")
			{
				Console.WriteLine("Tlačidlo 'Ďalšia stránka' je deaktivované – končím");
				break;
			}

			Console.WriteLine("Klikám na 'Ďalšia stránka'");
			await nextButton.ClickAsync();

			pageIndex++;
			await page.WaitForTimeoutAsync(800);
		}

		Console.WriteLine($"\n Hotovo. Celkom uložených záznamov: {totalRows}");
	}

	static string Csv(string value)
		=> $"\"{value.Replace("\"", "\"\"")}\"";
}
