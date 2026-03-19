using System;
using System.Collections.Generic;
using System.Text;

namespace OnomasticsDataSystem.Core.Models
{
	public class TopListStats
	{
		public string DisplayName { get; set; } = "";
		public List<TopItem> Items { get; set; } = new();
	}
}
