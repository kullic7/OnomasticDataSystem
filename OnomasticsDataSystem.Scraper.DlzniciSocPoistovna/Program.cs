using Microsoft.Playwright;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

class Program
{
	static async Task Main()
	{
		string url = "https://www.socpoist.sk/nastroje-sluzby/zoznam-dlznikov";

		Console.WriteLine("Spúšťam Playwright...");

		using var playwright = await Playwright.CreateAsync();
		await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = false,
			SlowMo = 20
		});

		var page = await browser.NewPageAsync();

		Console.WriteLine("Načítavam stránku...");
		await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

		Console.WriteLine("Hľadám link...");

		var link = page.Locator("a[href^='/api/idsp/download']").First;
		await link.WaitForAsync();

		string href = await link.GetAttributeAsync("href");
		string csvZipUrl = href.StartsWith("/") ? $"https://www.socpoist.sk{href}" : href;

		Console.WriteLine("Nájdený ZIP:");
		Console.WriteLine(csvZipUrl);

		Console.WriteLine("Sťahujem ZIP...");
		byte[] zipBytes = await DownloadBinary(csvZipUrl);

		Console.WriteLine("Rozbaľujem ZIP...");

		string csvContent = ExtractCsvFromZip(zipBytes);

		Console.WriteLine("Spracovávam CSV...");

		var cleaned = ProcessCsv(csvContent);

		File.WriteAllLines("dlznici_clean.csv", cleaned, Encoding.UTF8);

		Console.WriteLine("HOTOVO → dlznici_clean.csv");
	}

	static async Task<byte[]> DownloadBinary(string url)
	{
		using var client = new HttpClient();
		client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
		return await client.GetByteArrayAsync(url);
	}

	static string ExtractCsvFromZip(byte[] zipBytes)
	{
		using var ms = new MemoryStream(zipBytes);
		using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

		var entry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".csv"));

		if (entry == null)
			throw new Exception("ZIP neobsahuje CSV súbor!");

		using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
		return reader.ReadToEnd();
	}

	static List<string> ProcessCsv(string csv)
	{
		var rows = new List<string> { "Meno;Priezvisko;Mesto" };

		using var reader = new StringReader(csv);

		var config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			Delimiter = ",",
			HasHeaderRecord = true,
			Encoding = Encoding.UTF8,
			BadDataFound = null,
			MissingFieldFound = null
		};

		using var csvReader = new CsvReader(reader, config);

		while (csvReader.Read())
		{
			string name = csvReader.GetField(0)?.Trim();
			string city = csvReader.GetField(3)?.Trim();

			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(city))
				continue;

			if (IsCompany(name))
				continue;

			name = RemoveTitles(name);

			var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2) continue;

			string priezvisko = parts[0];
			string meno = string.Join(" ", parts.Skip(1));

			rows.Add($"{meno};{priezvisko};{city}");
		}

		return rows;
	}

	static bool IsCompany(string name)
	{
		string lower = name.ToLower();
		string[] markers = { "s.r.o", "sro", "a.s", "v.o.s", "k.s", "ltd", "gmbh", "kft", "spol.", "n.o" };
		return markers.Any(m => lower.Contains(m));
	}

	static string RemoveTitles(string name)
	{
		string[] titles = { "ing", "mgr", "bc", "judr", "phdr", "mudr", "phd", "prof", "doc", "rndr" };

		foreach (var t in titles)
			name = Regex.Replace(name, $@"\b{t}\.?\b", "", RegexOptions.IgnoreCase);

		return Regex.Replace(name, @"\s+", " ").Trim();
	}
}
