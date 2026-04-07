using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
	static async Task Main()
	{
		string baseUrl = "https://absolventi.uniba.sk/api/search?filter=%22%22&sortOrder=asc&sortBy=name&pageNumber={0}&pageSize={1}";
		using var http = new HttpClient();

		string csvPath = "absolventi.csv";

		using var writer = new StreamWriter(csvPath, false, new UTF8Encoding(true));
		writer.WriteLine("Name;LastName;BirthYear;BirthPlace");

		int page = 1;
		int pageSize = 10000; // veľkosť jednej stránky 
		int total = 0; // počet spracovaných záznamov
		
		int totalCount = 0;
		// Najprv načítaj malú odpoveď len kvôli získaniu `count` (celkový počet záznamov)
		string firstUrl = string.Format(baseUrl, 1, 1);
		string firstResponse = await http.GetStringAsync(firstUrl);
		using (var firstJson = JsonDocument.Parse(firstResponse))
		{
			if (firstJson.RootElement.TryGetProperty("count", out var countEl))
				totalCount = countEl.GetInt32();
		}
		Console.WriteLine("Sťahujem dáta");
		int totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
		if (totalPages == 0)
		{
			Console.WriteLine("Žiadne dáta na stiahnutie.");
			return;
		}
		Console.WriteLine($"Celkový počet záznamov: {totalCount}, strán: {totalPages}");
		while (page <= totalPages)
		{
			string url = string.Format(baseUrl, page, pageSize);
			string response;
			try
			{
				response = await http.GetStringAsync(url);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Chyba pri načítaní strany {page}: {ex.Message}. Skúšam znova...");
				await Task.Delay(2000);
				continue;
			}
			// Parsovanie odpovede do JsonDocument a overenie očakávanej štruktúry
			using var jsonDoc = JsonDocument.Parse(response);
			var root = jsonDoc.RootElement;

			if (!root.TryGetProperty("data", out var data))
			{
				Console.WriteLine($"Pole 'data' sa nenašlo na stránke {page} – koniec.");
				break;
			}
			if (data.ValueKind != JsonValueKind.Array)
			{
				Console.WriteLine($"Neplatná štruktúra dát na stránke {page} – koniec.");
				break;
			}
			int count = data.GetArrayLength();
			if (count == 0)
			{
				Console.WriteLine($"Hotovo! Na stránke {page} už neboli žiadne dáta.");
				break;
			}
			foreach (var item in data.EnumerateArray())
			{
				string name = item.GetProperty("name").GetString() ?? "";
				string lastName = item.GetProperty("last_name").GetString() ?? "";
				string birthPlace = item.GetProperty("birth_place").GetString() ?? "";
				string birthDate = item.GetProperty("birth_date").GetString() ?? "";
				string birthYear = birthDate.Length >= 4 ? birthDate[..4] : "";
				writer.WriteLine($"\"{name}\";\"{lastName}\";\"{birthYear}\";\"{birthPlace}\"");
			}
			total += count;
			Console.WriteLine($"Stránka {page} – {count} záznamov (celkom {total})");
			page++;
			await Task.Delay(200);
		}
		Console.WriteLine($"Načítaných {total} záznamov, uložené do {csvPath}");
	}
}