using OnomasticsDataSystem.Common;
using OnomasticsDataSystem.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnomasticDataSystem.DataImport.Cleaning
{
	public class AbsolventiUnibaCleaner
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

					if (columns.Length < 4)
						continue;

					var rawFirstName = Helper.GetFirstWord(columns[0].Trim());
					var rawLastName = Helper.GetFirstWord(columns[1].Trim());
					if (Helper.IsInvalidValue(rawFirstName) ||
							Helper.IsInvalidValue(rawLastName))
					{
						continue; // preskočí celý riadok
					}
					
					var (firstName, lastName) = Helper.FixSwappedNames(rawFirstName, rawLastName);

					firstName = Helper.FormatName(firstName);
					lastName = Helper.FormatName(lastName);

					firstName = Helper.NormalizeFirstName(firstName);
					var firstNameNorm = Helper.Normalize(firstName);
					var lastNameNorm = Helper.Normalize(lastName);

					int? birthYear = Helper.ParseValidBirthYear(columns[2]);

					var cleanBirthPlace = Helper.CleanAndNormalizeCity(columns[3].Trim());
					
					var normalizeBirthPlace = Helper.Normalize(cleanBirthPlace);
					

					var uniqueKey = (
						firstNameNorm,
						lastNameNorm,
						birthYear,
						normalizeBirthPlace
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
