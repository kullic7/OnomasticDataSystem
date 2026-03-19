using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnomasticsDataSystem.Core.Entity
{
	public class Source
	{
		public int Id { get; set; }

		public string Url { get; set; } = null!;

		public string Name { get; set; } = null!;

		public DateTime CreatedAt { get; set; }

		public ICollection<Person> Persons { get; set; } = new List<Person>();
	}

}
