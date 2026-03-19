using System;
using System.Collections.Generic;
using System.Text;

namespace OnomasticsDataSystem.Core.Models
{
	public class DashboardStatsForHome
	{
		public int TotalPersons { get; set; }
		public int UniqueNames { get; set; }
		public int UniqueSurnames { get; set; }
		public int UniqueCities { get; set; }

		public List<TopItem> TopNames { get; set; } = new();
		public List<TopItem> TopSurnames { get; set; } = new();
		public List<TopItem> TopCities { get; set; } = new();
	}
}
