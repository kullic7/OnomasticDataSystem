using System;
using System.Collections.Generic;
using System.Text;

namespace OnomasticsDataSystem.Core.Models
{
	public class TopItem
	{
		public int Rank { get; set; }

		public string Name { get; set; } = "";

		public int Value { get; set; }

		public double Percent { get; set; }
	}
}
