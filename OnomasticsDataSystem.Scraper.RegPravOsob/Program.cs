
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

class Program
{
	static async Task Main()
	{
		string baseUrl = "https://frkqbrydxwdp.compat.objectstorage.eu-frankfurt-1.oraclecloud.com/susr-rpo/";
		string prefix = "batch-init/init_2025-11-01_";
		string downloadDir = "downloads";
		string outputDir = "output";

		Directory.CreateDirectory(downloadDir);
		Directory.CreateDirectory(outputDir);

		using var client = new HttpClient();

		Console.WriteLine("Načítavam zoznam objektov");
		var xml = await client.GetStringAsync(baseUrl + "?prefix=" + prefix);
		var doc = XDocument.Parse(xml);
		XNamespace ns = "http://s3.amazonaws.com/doc/2006-03-01/";

		var keys = new List<string>();
		foreach (var content in doc.Descendants(ns + "Contents"))
		{
			string key = content.Element(ns + "Key")?.Value ?? "";
			if (key.Contains(prefix) && key.EndsWith(".json.gz"))
				keys.Add(key);
		}
		Console.WriteLine($"Našiel som {keys.Count} súborov na spracovanie.");

		foreach (var key in keys)
		{
			string url = baseUrl + key;
			string baseName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(key));
			string localPath = Path.Combine(downloadDir, baseName + ".json.gz");
			string jsonPath = Path.Combine(downloadDir, baseName + ".json");
			string csvPath = Path.Combine(outputDir, baseName + ".csv");

			Console.WriteLine($"Sťahujem {baseName}");
			using (var stream = await client.GetStreamAsync(url))
			using (var fileStream = File.Create(localPath))
				await stream.CopyToAsync(fileStream);

			Console.WriteLine($"Rozbaľujem {baseName}");
			using (var compressed = File.OpenRead(localPath))
			using (var gzip = new GZipStream(compressed, CompressionMode.Decompress))
			using (var output = File.Create(jsonPath))
				await gzip.CopyToAsync(output);

			Console.WriteLine($"Spracúvam {baseName}");

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			using var reader = new JsonTextReader(new StreamReader(jsonPath));
			using var writer = new StreamWriter(csvPath, false, new UTF8Encoding(true));
			writer.WriteLine("Id;Meno;Priezvisko;Mesto;Krajina");

			string? currentRecordId = null;    
			string? given = null, family = null, city = null, country = null, prop = null;
			while (await reader.ReadAsync())
			{
				if (reader.TokenType == JsonToken.PropertyName)
				{
					prop = (string)reader.Value!;
				}
				else if (prop != null)
				{
					switch (prop)
					{
						case "id":
							// Toto je hlavné ID — ČÍTAJ NA ÚROVNI result objektu
							if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.String)
							{
								// id vo vnútri personName ignorujeme – má inú Path
								if (reader.Path.Count(c => c == '.') == 1)
									currentRecordId = reader.Value?.ToString();
							}
							break;
						case "givenNames":
							if (reader.TokenType == JsonToken.String)
								given = reader.Value?.ToString();
							break;
						case "familyNames":
							if (reader.TokenType == JsonToken.String)
								family = reader.Value?.ToString();
							break;
						case "value":
							if (reader.TokenType == JsonToken.String)
							{
								string val = reader.Value?.ToString() ?? "";

								if (reader.Path.Contains("municipality"))
								{
									city = val.Split(new[] { " - ", "-", "–", "—" }, StringSplitOptions.None)[0].Trim();
								}
								else if (reader.Path.Contains("country"))
								{
									country = val;
								}
							}
							break;
					}
				}

				if (reader.TokenType == JsonToken.EndObject && reader.Path.EndsWith("personName"))
				{
					if (!string.IsNullOrWhiteSpace(currentRecordId) &&
						!string.IsNullOrWhiteSpace(given) &&
						!string.IsNullOrWhiteSpace(family) &&
						!string.IsNullOrWhiteSpace(city) &&
						string.Equals(country, "Slovenská republika", StringComparison.OrdinalIgnoreCase))
					{
						string keyEntry = $"{currentRecordId}|{given}|{family}";
						if (seen.Add(keyEntry))
						{
							writer.WriteLine($"{currentRecordId};{given};{family};{city};{country}");
						}
					}
					// Reset iba osoby – NIE currentRecordId
					given = family = city = country = prop = null;
				}
				// koniec celého result objektu → reset ID
				if (reader.TokenType == JsonToken.EndObject && reader.Path == "results")
				{
					currentRecordId = null;
				}
			}
			writer.Flush();
			Console.WriteLine($"Hotovo: {csvPath} ({seen.Count} unikátnych záznamov)");
		}
		Console.WriteLine("Všetky CSV uložené v priečinku 'output'");
	}
}