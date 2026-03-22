using System.Globalization;
using System.Text;

namespace OnomasticsDataSystem.Common
{
	public static class Helper
	{
		public static Dictionary<string, string> NameMap = new(StringComparer.OrdinalIgnoreCase);
		private static readonly HashSet<string> SlovakCities = new HashSet<string>
		{
			"Bratislava","Košice","Prešov","Žilina","Nitra","Banská Bystrica","Trnava","Trenčín","Martin","Poprad",
			"Prievidza","Zvolen","Považská Bystrica","Michalovce","Nové Zámky","Spišská Nová Ves","Komárno","Humenné",
			"Levice","Bardejov","Liptovský Mikuláš","Lučenec","Piešťany","Ružomberok","Trebišov","Topoľčany","Čadca",
			"Dubnica nad Váhom","Rimavská Sobota","Partizánske","Dunajská Streda","Vranov nad Topľou","Hlohovec",
			"Šaľa","Senica","Pezinok","Malacky","Skalica","Bánovce nad Bebravou","Snina","Rožňava","Dolný Kubín",
			"Žiar nad Hronom","Púchov","Handlová","Stará Ľubovňa","Svidník","Galanta","Kežmarok","Sered",
			"Detva","Šamorín","Stupava","Sabinov","Štúrovo","Revúca","Veľký Krtíš","Kysucké Nové Mesto",
			"Nová Dubnica","Bytča","Holíč","Levoča","Myjava","Nová Baňa","Gbely","Rajec","Kremnica",
			"Tisovec","Modra","Vysoké Tatry","Svätý Jur","Turčianske Teplice","Nové Mesto nad Váhom",
			"Ilava","Námestovo","Hriňová","Turany","Krásno nad Kysucou","Šaštín-Stráže","Giraltovce",
			"Medzilaborce","Dobšiná","Krupina","Dudince","Tornaľa","Fiľakovo","Hnúšťa","Poltár",
			"Sládkovičovo","Veľký Meder","Gabčíkovo","Kolárovo","Šurany","Šahy","Tlmače","Želiezovce",
			"Šurany","Vrútky","Turzovka","Čierna nad Tisou","Strážske","Sečovce","Veľké Kapušany",
			"Sobrance","Moldava nad Bodvou","Medzev","Spišské Vlachy","Spišská Belá","Spišské Podhradie",
			"Podolínec","Stará Turá","Brezno","Banská Štiavnica","Žarnovica","Nováky","Bojnice",
			"Chlmec","Veľký Šariš","Lipany","Stropkov","Sabinov","Svit","Lendak","Tvrdošín","Zlaté Moravce",
			"Trstená","Dolný Kubín","Nesvady"
		};
		private static readonly HashSet<string> UnwantedValues = new(StringComparer.OrdinalIgnoreCase)
		{
			"Ing.",
			"Mgr.",
			"Bc.",
			"Doc.",
			"PhDr.",
			"MUDr.",
			"RNDr.",
			"JUDr.",
			"Prof.",
			"PhD",
			"Dr.",
			"Bez",
			"hrobka",
			"névtelen",
			"olvashatatlan",
			"rodičia",
			"nie",
			"hrob",
			"pomníka",
			"rodina",
			"Neznámy",
			"tráva",
			"Voľné",
			"voľné",
			"zem",
			"kvety",
			"HM",
			"predk.",
			"úrad",
			"Nečitateľné",
			"ZDRUŽENIE",
			"s. r. o.",
			"s.r.o.",
			"spol.",
			"služba",
			"",
			"&",
			"Group",
			"Slovakia",
			"HOLDING",
			"Slovensko",
			"SLOVAKIA,",
			"komuni",
			"Capital",
			"CREDIT",
			"s.",
			"EU",
			"SK",
			"urna",
			"Miesto",
			"rezervovane",
			"organizacia",
			"Deti",
			"Umrel",
			"Umrela",
			"Umreli",
			"Mrtvonarodené",
			"Mŕtvonarodený",
			"Poľnohospodárske",
			"Reality"

		};
		
		public static string? Normalize(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			var lower = value.Trim().ToLowerInvariant();

			var normalized = lower.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalized)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder
				.ToString()
				.Normalize(NormalizationForm.FormC);
		}
		public static bool IsInvalidValue(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return true;

			var trimmed = value.Trim();
			// ❗ odstráň všetky whitespace znaky
			trimmed = new string(trimmed
				.Where(c => !char.IsWhiteSpace(c))
				.ToArray());
			// príliš krátke (napr. "A", "Jo")
			if (trimmed.Length < 3)
				return true;

			// kontrola zoznamu nežiaducich hodnôt (napr. "nezistené", "unknown", atď.)
			if (UnwantedValues.Contains(trimmed))
				return true;

			// kontrola nežiaducich znakov
			return trimmed.Any(c =>
				char.IsDigit(c) ||
				c == '-' ||
				c == '–' ||
				c == '—' ||
				c == '−' ||
				c == '#' ||
				c == '.' ||
				c == '/' ||
				c == '+' ||
				c == '?' ||
				c == ',' ||
				c == '(' ||
				c == ')' ||
				c == '&');
		}
		public static int? ParseValidBirthYear(string value)
		{
			if (!int.TryParse(value?.Trim(), out int year))
				return null;

			int currentYear = DateTime.Now.Year;

			if (year < 1800 || year > currentYear)
				return null;

			return year;
		}

		public static string GetFirstWord(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return string.Empty;

			return input
				.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				.FirstOrDefault() ?? string.Empty;
		}

		public static bool ContainsFullyUppercaseWord(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return false;

			var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			foreach (var word in words)
			{
				var letters = word.Where(char.IsLetter).ToList();

				// musí obsahovať písmená
				if (!letters.Any())
					continue;

				// všetky písmená sú veľké
				if (letters.All(char.IsUpper))
					return true;
			}

			return false;
		}
		public static string? CleanAndNormalizeCity(string? rawCity)
		{
			if (string.IsNullOrWhiteSpace(rawCity))
				return null;

			// 1. odstránenie bordelu (čísla, / atď.)
			var cleaned = string.Join(" ",
				rawCity
					.Split(' ', StringSplitOptions.RemoveEmptyEntries)
					.Where(part =>
						!part.Contains('/') &&
						!part.Any(char.IsDigit)));

			if (string.IsNullOrWhiteSpace(cleaned))
				return null;

			var normalizedValue = Normalize(cleaned);

			// 2. pokus o mapovanie na známe mesto
			foreach (var city in SlovakCities)
			{
				if (normalizedValue!.Contains(Normalize(city)))
				{
					return FormatCity(city);
				}
			}

			// 3. fallback → len naformátuj cleaned hodnotu
			return FormatCity(cleaned);
		}
		private static string FormatCity(string value)
		{
			return string.Join(" ",
				value.Trim()
					 .ToLowerInvariant()
					 .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
					 .Select(w => char.ToUpper(w[0]) + w.Substring(1)));
		}

		private static bool IsLikelyLastName(string value)
		{
			var lower = value.ToLowerInvariant();

			return
				// ženske
				lower.EndsWith("ová") ||
				lower.EndsWith("ova") ||
				lower.EndsWith("očná") ||

				// men
				lower.EndsWith("ák") ||
				lower.EndsWith("ak") ||
				lower.EndsWith("až") ||
				lower.EndsWith("az") ||
				lower.EndsWith("vač") || 
				lower.EndsWith("vič") ||
				lower.EndsWith("oj") ||
				lower.EndsWith("ajz") ||
				lower.EndsWith("vac");
		}
		public static (string firstName, string lastName) FixSwappedNames(string firstName, string lastName)
		{
			if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
				return (firstName, lastName);

			var f = firstName.Trim();
			var l = lastName.Trim();

			// ak first vyzerá ako priezvisko a last nie → swap
			if (IsLikelyLastName(f) && !IsLikelyLastName(l))
			{
				return (l, f);
			}

			return (f, l);
		}
		public static string? FormatName(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			var trimmed = value.Trim().ToLowerInvariant();

			return char.ToUpper(trimmed[0]) + trimmed.Substring(1);
		}
		public static string? NormalizeFirstName(string? name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;

			var normalized = Normalize(name);

			if (normalized != null && NameMap.TryGetValue(normalized, out var canonical))
			{
				return canonical;
			}

			return FormatName(name);
		}
		public static void LoadNamesFromCsv(string path)
		{
			if (!File.Exists(path))
				return;

			foreach (var line in File.ReadLines(path).Skip(1))
			{
				var parts = line.Split(';');

				if (parts.Length < 2)
					continue;

				var variant = Normalize(parts[0]);
				var canonical = FormatName(parts[1]);

				if (!string.IsNullOrWhiteSpace(variant) && !NameMap.ContainsKey(variant))
				{
					NameMap.Add(variant, canonical!);
				}
			}
		}
	}
}
