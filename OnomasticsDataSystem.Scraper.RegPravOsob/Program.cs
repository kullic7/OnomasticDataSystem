
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

			string? currentRecordId = null;     // HLAVNÉ ID
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

				// Koniec jedného personName objektu = JEDNA OSOBA
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





//using System.IO.Compression;
//using System.Text;
//using System.Xml.Linq;
//using Newtonsoft.Json;

//class Program
//{
//	static async Task Main()
//	{
//		string baseUrl = "https://frkqbrydxwdp.compat.objectstorage.eu-frankfurt-1.oraclecloud.com/susr-rpo/";
//		string prefix = "batch-init/init_2025-11-01_";
//		string downloadDir = "downloads";
//		string outputDir = "output";

//		//CreateDirectory() zabezpeci, že priečinky existujú (nevypíše chybu ak už existujú).
//		Directory.CreateDirectory(downloadDir);
//		Directory.CreateDirectory(outputDir);

//		using var client = new HttpClient();// vytvorí HttpClient na sťahovanie súborov a XML

//		Console.WriteLine("Načítavam zoznam objektov");
//		var xml = await client.GetStringAsync(baseUrl + "?prefix=" + prefix);
//		var doc = XDocument.Parse(xml);
//		XNamespace ns = "http://s3.amazonaws.com/doc/2006-03-01/";

//		var keys = new List<string>();
//		foreach (var content in doc.Descendants(ns + "Contents"))//prejde celý strom XML (rekurzívne do hĺbky) a vráti všetky elementy <Contents>
//		{
//			string key = content.Element(ns + "Key")?.Value ?? "";// získa hodnotu elementu <Key> alebo prázdny reťazec
//			if (key.Contains(prefix) && key.EndsWith(".json.gz"))// skontroluje, či kľúč obsahuje prefix a končí .json.gz
//				keys.Add(key);
//		}

//		Console.WriteLine($"Našiel som {keys.Count} súborov na spracovanie.");

//		foreach (var key in keys)
//		{
//			string url = baseUrl + key;
//			string baseName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(key));
//			string localPath = Path.Combine(downloadDir, baseName + ".json.gz");
//			string jsonPath = Path.Combine(downloadDir, baseName + ".json");
//			string csvPath = Path.Combine(outputDir, baseName + ".csv");

//			Console.WriteLine($"Sťahujem {baseName}");
//			using (var stream = await client.GetStreamAsync(url)) // otvorí stream zo vzdialenej URL
//			using (var fileStream = File.Create(localPath)) // otvorí súbor na zápis pre uloženie stiahnutého obsahu
//				await stream.CopyToAsync(fileStream); // skopíruje obsah zo streamu do súboru

//			Console.WriteLine($"Rozbaľujem {baseName}");
//			using (var compressed = File.OpenRead(localPath)) // otvorí stiahnutý .json.gz súbor na čítanie
//			using (var gzip = new GZipStream(compressed, CompressionMode.Decompress)) // vytvorí GZipStream na dekompresiu
//			using (var output = File.Create(jsonPath)) // otvorí súbor pre zapis rozbaleného JSONu
//				await gzip.CopyToAsync(output); // skopíruje dekomprimované dáta do JSON súboru

//			Console.WriteLine($"Spracúvam {baseName}");

//			//deduplikacia
//			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

//			using var reader = new JsonTextReader(new StreamReader(jsonPath));
//			using var writer = new StreamWriter(csvPath, false, new UTF8Encoding(true));
//			writer.WriteLine("Meno;Priezvisko;Mesto;Krajina");

//			string? given = null, family = null, city = null, country = null, prop = null;

//			while (await reader.ReadAsync())
//			{
//				if (reader.TokenType == JsonToken.PropertyName) // ak token je názov vlastnosti
//				{
//					prop = (string)reader.Value!; // uloží názov vlastnosti do premennej prop
//				}
//				else if (reader.TokenType == JsonToken.String && prop != null) // ak token je reťazcová hodnota a máme názov vlastnosti
//				{
//					switch (prop) // vyhodnotí podľa názvu vlastnosti, kam uložiť hodnotu
//					{
//						case "givenNames": 
//							given = reader.Value?.ToString(); 
//							break; 
//						case "familyNames": 
//							family = reader.Value?.ToString(); 
//							break; 
//						case "value": // ak je názov vlastnosti "value"
//							if (reader.Path.Contains("municipality"))
//							{ // ak cesta v JSON naznačuje, že ide o obec / mesto
//								var rawCity = reader.Value?.ToString() ?? "";
//								city = rawCity.Split(new[] { " - ", "-", "–", "—" }, StringSplitOptions.None)[0].Trim();
//							}
//							else if (reader.Path.Contains("country")) // ak cesta naznačuje krajinu
//								country = reader.Value?.ToString(); 
//							break;
//					}
//				}

//				// Keď sa ukončí objekt osoby
//				if (reader.TokenType == JsonToken.EndObject && reader.Path.EndsWith("personName"))
//				{
//					if (!string.IsNullOrWhiteSpace(given) &&
//						!string.IsNullOrWhiteSpace(family) &&
//						!string.IsNullOrWhiteSpace(city) &&
//						string.Equals(country, "Slovenská republika", StringComparison.OrdinalIgnoreCase))
//					{
//						string keyEntry = $"{given}|{family}|{city}|{country}";
//						if (seen.Add(keyEntry)) // zapíše iba ak ešte neexistuje
//						{
//							writer.WriteLine($"{given};{family};{city};{country};");
//						}
//					}
//					//reset premenných pre ďalší záznam
//					given = family = city = country = prop = null;
//				}
//			}

//			writer.Flush();
//			Console.WriteLine($" Hotovo: {csvPath} ({seen.Count} unikátnych záznamov)");
//		}

//		Console.WriteLine("Všetky CSV uložené v priečinku 'output'");
//	}
//}
