using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnomasticsDataSystem.Core.Entity
{

	public class Person
	{
		public int Id { get; set; }

		public string FirstName { get; set; } = null!;
		public string LastName { get; set; } = null!;

		public string? FirstNameNormalized { get; set; }
		public string? LastNameNormalized { get; set; }

		public int? BirthYear { get; set; }
		public string? BirthCity { get; set; }
		public string? BirthCityNormalized { get; set; }

		public DateTime CreatedAt { get; set; }

		// Foreign Key
		public int SourceId { get; set; }

		public Source Source { get; set; } = null!;
	}
}
