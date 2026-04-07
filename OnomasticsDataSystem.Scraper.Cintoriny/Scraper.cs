using Microsoft.Playwright;
using OnomasticsDataSystem.Core.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace OnomasticsDataSystem.Scraper.Cintoriny
{
	public class Scraper
	{
		private readonly HttpClient _client;
		private int? _lastCid = null;
		public Scraper()
		{
			var handler = new HttpClientHandler
			{
				CookieContainer = new CookieContainer(),
				UseCookies = true
			};
			_client = new HttpClient(handler);
			_client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (cintoriny)");
		}
	
		public async Task RunAsync()
		{
			string searchUrl = "https://www.cintoriny.sk/src/index.php?simpleSearch&type=dostupnost";
			Console.WriteLine("Spúšťam scraper");

			//Získaj všetky CID
			var allCids = await FindCemeteryIds(searchUrl);
			Console.WriteLine($"Nájdených CID: {allCids.Count}");
			if (allCids.Count == 0)
			{
				Console.WriteLine("Nenašli sa žiadne CID.");
				return;
			}

			for (int cidIndex = 0; cidIndex < allCids.Count; cidIndex++)
			{
				var c = allCids[cidIndex];

				Console.WriteLine($"\n CID {c} ");

				//Názvy miest podľa CID
				var cityDict = await GetCityNamesAsync("https://www.cintoriny.sk/api/getMapInfo.php", c);

				//Nájdeme identifikátory
				var identifiers = await FindIdentifiers("https://www.cintoriny.sk/api/getMap.php", c);

				if (identifiers.Count == 0)
				{
					Console.WriteLine($"[CID {c}] Žiadne identifikátory.");
					continue;
				}

				Console.WriteLine($"[CID {c}] Načítaných identifikátorov: {identifiers.Count}");

				//Lokálny zoznam osôb pre tento CID
				var outLines = new ConcurrentBag<Person>();

				//max 2 paralelné requesty
				var semaphore = new SemaphoreSlim(2);
				var tasks = new List<Task>();
				int counter = 0;

				//Cyklus cez identifikátory
				for (int idIndex = 0; idIndex < identifiers.Count; idIndex++)
				{
					string identifier = identifiers[idIndex];

					await semaphore.WaitAsync();

					var task = Task.Run(async () =>
					{
						try
						{
							var persons = await FindPopupHtml(int.Parse(c), identifier);

							foreach (var pers in persons)
							{
								int index = Interlocked.Increment(ref counter);
								pers.BirthCity = cityDict[c];

								Console.WriteLine($"{index}; CID={c}; ID={identifier}; {pers}");

								outLines.Add(pers);
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"[CID {c} | {identifier}] tuuu1Chyba: {ex.Message}");
						}
						finally
						{
							semaphore.Release();
						}
					});
					tasks.Add(task);
				}
				await Task.WhenAll(tasks);
				var fileName = $"cid_{c}.csv";
				WriteCsv(fileName, outLines);
				Console.WriteLine($"[CID {c}] Dokončené → uložené do: {fileName}");
			}
			Console.WriteLine($"\nHotovo.Celkovo spracovaných  osôb.");
		}

		private async Task WriteCsv(string path, IEnumerable<Person> rows)
		{
			var utf8Bom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
			using var sw = new StreamWriter(path, append: false, encoding: utf8Bom);
			await sw.WriteLineAsync("FirstName;LastName;BirthCity;BirthYear");

			string Q(string s)
			{
				if (string.IsNullOrEmpty(s)) return "";
				var needsQuotes = s.IndexOfAny(new[] { ';', '"', '\r', '\n' }) >= 0;
				s = s.Replace("\"", "\"\"");
				return needsQuotes ? $"\"{s}\"" : s;
			}

			foreach (var r in rows)
				await sw.WriteLineAsync($"{Q(r.FirstName)};{Q(r.LastName)};{Q(r.BirthCity)}" + ";" + r.BirthYear);
		}

		public async Task<List<string>> FindIdentifiers(string url, string cidValue)
		{
			var identifiers = new List<string>();
			int attempts = 0;
			while (attempts < 3)
			{
				try
				{
					attempts++;
					//GET session init
					var homeUrl = $"https://www.cintoriny.sk/src/index.php?type=home&cintorinId={cidValue}";
					var resp1 = await _client.GetAsync(homeUrl);
					resp1.EnsureSuccessStatusCode();
					//POST getMap.php
					var form = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string,string>("cId", cidValue)
					});

					var resp2 = await _client.PostAsync(url, form);
					resp2.EnsureSuccessStatusCode();

					var body = await resp2.Content.ReadAsStringAsync();

					foreach (Match m in Regex.Matches(body, @"hrob=([^\r\n&]+)", RegexOptions.IgnoreCase))
					{
						var ident = m.Groups[1].Value.Trim();
						// 1) viac čísiel oddelených čiarkou
						if (Regex.IsMatch(ident, @"^(?=.*\s)(?=.*,)\d[\d\s.]*\.\s*\d+(?:,\d+)*$"))
						{
							Console.WriteLine($"som tu0");
							var baseMatch = Regex.Match(ident, @"^(.*?\. *)(?:\d+(?:,\d+)*)$");
							if (baseMatch.Success)
							{
								var prefix = baseMatch.Groups[1].Value;
								var numbers = Regex.Matches(ident, @"\d+");

								foreach (Match num in numbers)
								{
									var fullIdent = $"{prefix}{num.Value}";
									fullIdent = fullIdent.Replace(".", "");
									fullIdent = Regex.Replace(fullIdent, @"\s+", "____");
									identifiers.Add(fullIdent);
								}
							}
							else
							{
								ident = ident.Replace(".", "");
								ident = Regex.Replace(ident, @"\s+", "____");
								identifiers.Add(ident);
							}
						}
						// typ: "1..123" -> "1____123"
						else if (Regex.IsMatch(ident, @"^\d+\.\.+\d+$"))
						{
							Console.WriteLine($"som tu1");
							var parts = Regex.Matches(ident, @"\d+");
							if (parts.Count == 2)
							{
								identifiers.Add($"{parts[0].Value}____{parts[1].Value}");
							}
							else
							{
								ident = ident.Replace(".", "");
								ident = Regex.Replace(ident, @"\s+", "____");
								identifiers.Add(ident);
							}
						}
						// typ: 1.8.1,2 (bodky + viac čísel oddelených čiarkou)
						else if (ident.Contains(".") && ident.Contains(",") && !ident.Contains(" "))
						{
							var lastDot = ident.LastIndexOf('.');
							var prefix = ident.Substring(0, lastDot + 1);  
							var suffix = ident.Substring(lastDot + 1); 
							// Rozbieme suffix podľa ","
							var nums = suffix.Split(',', StringSplitOptions.RemoveEmptyEntries);
							foreach (var n in nums)
							{
								var full = $"{prefix}{n}";
								full = full.Replace(".", "__");                    
								full = Regex.Replace(full, @"\s+", "__");      
								identifiers.Add(full);
							}
							continue;
						}
						//typ: "11.  .171.  .a"
						else if (Regex.IsMatch(ident, @"^\d+\.\s*\.\s*[^,]+$"))
						{
							Console.WriteLine($"som tu3");
							var prefixMatch = Regex.Match(ident, @"^(\d+\.\s*\.)\s*(.+)$");
							if (prefixMatch.Success)
							{
								var prefix = prefixMatch.Groups[1].Value;
								var rest = prefixMatch.Groups[2].Value;
								var parts = Regex.Matches(rest, @"(\d+|[A-Za-z]+)");
								foreach (Match part in parts)
								{
									var fullIdent = $"{prefix}{part.Value}";
									fullIdent = fullIdent.Replace(".", "");
									fullIdent = Regex.Replace(fullIdent, @"\s+", "____");
									identifiers.Add(fullIdent);
								}
							}
							continue;
						}
						// typ: ABC.12.X – všetky časti môžu obsahovať čísla aj písmená
						else if (Regex.IsMatch(ident, @"^[A-Za-z0-9]+(\.[A-Za-z0-9]+)+$"))
						{
							Console.WriteLine($"som tu4");
							var parts = ident.Split('.', StringSplitOptions.RemoveEmptyEntries);
							// napr. 1.1.39.A.X → %join% → 1__1__39__A__X
							string fullIdent = string.Join("__", parts);
							identifiers.Add(fullIdent);
						}
						// klasický formát
						else
						{
							Console.WriteLine($"som tu5");
							ident = ident.Replace(".", "");
							ident = Regex.Replace(ident, @"\s+", "____");
							identifiers.Add(ident);
						}
					}

					// deduplikácia
					return identifiers
						.Where(x => !string.IsNullOrWhiteSpace(x))
						.Distinct()
						.ToList();
				}
				catch (HttpRequestException ex)
				{
					string msg = ex.Message;

					bool is500 =
						ex.StatusCode == HttpStatusCode.InternalServerError ||
						msg.Contains("500") || msg.Contains("Internal Server Error");

					if (is500)
					{
						Console.WriteLine($"[CID {cidValue}] ❗ Server Error 500 (pokus {attempts}/3)");
					}
					else
					{
						Console.WriteLine($"[CID {cidValue}] ⚠️ HTTP chyba: {msg} (pokus {attempts}/3)");
					}

					if (attempts >= 3)
					{
						Console.WriteLine($"[CID {cidValue}] ❌ Po 3 pokusoch vzdávam.");
						throw;
					}

					await Task.Delay(TimeSpan.FromMinutes(1));
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[CID {cidValue}] ❗ Iná chyba: {ex.Message}");

					if (attempts >= 3)
						throw;

					await Task.Delay(TimeSpan.FromMinutes(1));
				}
			}

			return identifiers;
		}

		public async Task<Dictionary<string, string>> GetCityNamesAsync(string infoUrl, string cid)
		{
			var results = new Dictionary<string, string>();
			int attempts = 0;
			while (attempts < 3)
			{
				try
				{
					attempts++;
					var homeUrl = $"https://www.cintoriny.sk/src/index.php?type=home&cintorinId={cid}";
					var resp1 = await _client.GetAsync(homeUrl);
					resp1.EnsureSuccessStatusCode();
					// POST na getMapInfo.php
					var form = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string,string>("cId", cid)
					});

					var resp2 = await _client.PostAsync(infoUrl, form);
					resp2.EnsureSuccessStatusCode();

					var bytes = await resp2.Content.ReadAsByteArrayAsync();
					var json = System.Text.Encoding.UTF8.GetString(bytes);

					using var doc = JsonDocument.Parse(json);
					var root = doc.RootElement;

					if (root.TryGetProperty("cityName", out var cityProp))
					{
						var raw = cityProp.GetString() ?? "";
						var decodedCity = Regex.Unescape(raw);
						results[cid] = decodedCity;
						Console.WriteLine($"{cid} → {decodedCity}");
					}

					return results;
				}
				catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.InternalServerError)
				{
					Console.WriteLine($"{cid} → Server Error 500 (pokus {attempts}/3).");

					if (attempts >= 3)
					{
						Console.WriteLine($"{cid} → ❌ Po 3 pokusoch sa vzdávam.");
						throw;
					}

					await Task.Delay(TimeSpan.FromMinutes(1));
				}
				catch (Exception ex)
				{
					Console.WriteLine($"{cid} → Iná chyba: {ex.Message}");

					if (attempts >= 3)
						throw;

					await Task.Delay(TimeSpan.FromMinutes(1));
				}
			}

			return results;
		}

		private async Task<List<string>> FindCemeteryIds(string url)
		{
			var foundIds = new HashSet<string>();
			using var playwright = await Playwright.CreateAsync();
			//spustí Chromium prehliadač v headless režime (bez okna)
			await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
			{
				Headless = true
			});
			var context = await browser.NewContextAsync();
			var page = await context.NewPageAsync();

			//zachytime vsetky odpovede a parsujeme id
			page.Response += async (_, response) =>
			{
				try
				{
					//ak odpoved nepochadza z cintoriny.sk ignorujeme
					if (!response.Url.Contains("cintoriny.sk", StringComparison.OrdinalIgnoreCase))
						return;

					string text = "";
					//pokusime sa ziskat obsah odpvedi
					try { text = await response.TextAsync(); } catch { return; }

					// regex na hľadanie ID
					foreach (Match m in Regex.Matches(text + response.Url,
						@"(?:\bcld|\bcid|\bcintorinId)[\s=:\-]+(\d{2,6})",
						RegexOptions.IgnoreCase))
					{
						foundIds.Add(m.Groups[1].Value);
					}

					// špeciálne: formát "cld 1240"
					foreach (Match m in Regex.Matches(text,
						@"\bcld\s+(\d{2,6})\b",
						RegexOptions.IgnoreCase))
					{
						foundIds.Add(m.Groups[1].Value);
					}
				}
				catch { }
			};
			await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
			await page.WaitForTimeoutAsync(3000);
			await browser.CloseAsync();
			return foundIds.OrderBy(x => int.Parse(x)).ToList();
		}

		private static List<Person> ExtractNames(string html)
		{
			var results = new List<Person>();
			if (string.IsNullOrWhiteSpace(html))
				return results;

			// Regex, ktorý nájde celý blok deceasedRow
			var rowRegex = new Regex(
				@"<div[^>]+class\s*=\s*['""]deceasedRow['""][^>]*>\s*<div[^>]*>(.*?)</div>\s*<div[^>]*>(.*?)</div>",
				RegexOptions.IgnoreCase | RegexOptions.Singleline);

			// Regex na odstránenie titulov
			var cleanupRegex = new Regex(@"\b(Ing|Sc|Csc|Rezervacia|Predkúpa|predkúpa|Rezervácia|Rod|Rod\.|Mgr|Dr|DrSc|MUDr|St|PhDr|JUDr|RNDr|MrPh|Bc|PhD|MDDr|doc|prof|Neznámy|Neznáma|Bez\s*mena|Bez\s*údajov|Pamätník|Hrob)\b\.?",
				RegexOptions.IgnoreCase);

			// Regex extrakcia rokov
			var yearRegex = new Regex(@"\*\s*(\d{4})(?:\s*[✝†]\s*(\d{4}))?", RegexOptions.IgnoreCase);

			foreach (Match m in rowRegex.Matches(html))
			{
				var namePart = m.Groups[1].Value;
				var datePart = m.Groups[2].Value;

				// FullNameRaw (celé meno bez zásahov)
				var rawNoTags = Regex.Replace(namePart, "<.*?>", string.Empty);
				var fullNameRaw = WebUtility.HtmlDecode(rawNoTags)?.Trim() ?? "";

				if (string.IsNullOrWhiteSpace(fullNameRaw))
					continue;

				// Pôvodné spracovanie mena (čistenie + delenie)
				var decoded = fullNameRaw;
				var cleaned = Regex.Replace(decoded, @"\s+", " ").Trim();

				// odstránenie titulov
				var filtered = cleanupRegex.Replace(cleaned, "").Trim();
				filtered = filtered.Trim(',', '.', ';', ':', '-', ' ');
				if (filtered.Length < 2)
					continue;
				var parts = filtered.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 0)
					continue;
				string firstName = parts[0];
				string lastName = parts.Length >= 2 ? parts[^1] : "";
				if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
					continue;

				// Rok narodenia
				int? birthYear = null;
				var yearMatch = yearRegex.Match(datePart);
				if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out var b))
					birthYear = b;

				results.Add(new Person
				{
					FirstName = firstName,
					LastName = lastName,
					BirthYear = birthYear
				});
			}
			return results;
		}

		private async Task<List<Person>> FindPopupHtml(int cid, string identifier)
		{
			var results = new List<Person>();
			int attempts = 0;
			while (attempts < 3)
			{
				try
				{
					attempts++;
					string homeUrl = $"https://www.cintoriny.sk/src/index.php?type=home&cintorinId={cid}";
					string popupUrl = "https://www.cintoriny.sk/api/htmlGetGravePopup.php";
					// Session GET len ak sa zmenil cintorín
					if (_lastCid != cid)
					{
						var resp1 = await _client.GetAsync(homeUrl);
						resp1.EnsureSuccessStatusCode();
						_client.DefaultRequestHeaders.Referrer = new Uri(homeUrl);
						_lastCid = cid;
					}

					// POST
					var form = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string,string>("cId", cid.ToString()),
						new KeyValuePair<string,string>("identifier", identifier)
					});

					var req = new HttpRequestMessage(HttpMethod.Post, popupUrl)
					{
						Content = form
					};
					req.Headers.Add("X-Requested-With", "XMLHttpRequest");

					var resp2 = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

					if (!resp2.IsSuccessStatusCode)
					{
						Console.WriteLine($"[CID {cid} | {identifier}] Server Error {resp2.StatusCode}");
						continue;
					}
					var html = await resp2.Content.ReadAsStringAsync();
					return ExtractNames(html);
				}
				catch (Exception ex)
				{
					bool is500 = ex.Message.Contains("500") || ex.Message.Contains("Internal Server Error");
					if (!is500)
						throw; 
					Console.WriteLine($"[CID {cid} | {identifier}]tuuu Server Error 500 (pokus {attempts}/3): {ex.Message}");

					if (attempts >= 3)
						throw;
					Console.WriteLine($"tuuuu");
					await Task.Delay(TimeSpan.FromMinutes(1));
				}
			}
			return results;
		}
	}
}