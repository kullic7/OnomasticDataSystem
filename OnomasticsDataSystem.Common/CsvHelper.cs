using System;
using System.Collections.Generic;
using System.Text;
using OnomasticsDataSystem.Core.Entity;

namespace OnomasticsDataSystem.Common
{
	public static class CsvHelper
	{
		public static string GeneratePersonsCsv(List<Person> data)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Id;FirstName;LastName;BirthCity;BirthYear");

			foreach (var p in data)
			{
				sb.AppendLine($"{p.Id};{p.FirstName};{p.LastName};{p.BirthCity};{p.BirthYear}");
			}

			return sb.ToString();
		}
	}
}
