using OnomasticsDataSystem.Common;
using OnomasticsDataSystem.Core.Entity;

namespace OnomasticDataSystem.DataImport.Cleaning
{
	public class CintorinyCleaner
	{
		public List<Person> Clean(string directoryPath)
		{
			var people = new List<Person>();
			var uniqueKeys = new HashSet<(string?, string?, int?, string?)>();

			if (!Directory.Exists(directoryPath))
				throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

			var files = Directory.GetFiles(directoryPath, "*.csv");

			foreach (var file in files)
			{
				var lines = File.ReadAllLines(file);

				foreach (var line in lines.Skip(1))
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;

					var columns = line.Split(';');

					if (columns.Length < 5)
						continue;

					var rawFirstName = columns[1].Trim();
					var rawLastName = columns[2].Trim();

					if (Helper.IsInvalidValue(rawFirstName) ||
							Helper.IsInvalidValue(rawLastName))
					{
						continue; // preskočí celý riadok
					}

					var firstName = Helper.GetFirstWord(rawFirstName);
					var lastName = Helper.GetFirstWord(rawLastName);
					(firstName, lastName) = Helper.FixSwappedNames(firstName, lastName);

					firstName = Helper.FormatName(firstName);
					lastName = Helper.FormatName(lastName);

					firstName = Helper.NormalizeFirstName(firstName);
					var firstNameNorm = Helper.Normalize(firstName);
					var lastNameNorm = Helper.Normalize(lastName);

					int? birthYear = Helper.ParseValidBirthYear(columns[4]);

					var cleanBirthPlace = Helper.CleanAndNormalizeCity(columns[3].Trim());
					
					var normalizeBirthPlace = Helper.Normalize(cleanBirthPlace);
				


					var uniqueKey = (
						firstNameNorm,
						lastNameNorm,
						birthYear,
						cleanBirthPlace?.ToLowerInvariant()
					);

					if (uniqueKeys.Contains(uniqueKey))
					{
						//Console.WriteLine(
						//	$"Duplicita nájdená: {firstName} {lastName}, {birthYear}, {cleanBirthPlace}"
						//);
						continue;
					}

					uniqueKeys.Add(uniqueKey);

					var person = new Person
					{
						FirstName = firstName,
						LastName = lastName,
						FirstNameNormalized = firstNameNorm,
						LastNameNormalized = lastNameNorm,
						BirthYear = birthYear,
						BirthCity = cleanBirthPlace,
						BirthCityNormalized = normalizeBirthPlace,
						CreatedAt = DateTime.UtcNow
					};

					people.Add(person);
				}
			}

			return people;
		}
	}
}
