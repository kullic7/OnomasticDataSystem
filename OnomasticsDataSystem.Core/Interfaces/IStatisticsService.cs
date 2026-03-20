using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnomasticsDataSystem.Core.Interfaces
{
	public interface IStatisticsService
	{

		Task<PagedResult<Person>> GetPersonsPage(
			int page,
			int pageSize,
			string? city,
			string? name,
			string? lastName,
			int? yearFrom,
			int? yearTo);

		Task<PagedResult<ItemStats>> GetNameStats(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith);
	
		Task<PagedResult<ItemStats>> GetSurnameStats(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		Task<PagedResult<ItemStats>> GetCityStats(int page, int pageSize, string? citySearch, int? minCount, int? maxCount);
		Task<TopListStats> GetNameCityStats(string name);
		Task<TopListStats> GetSurnameCityStats(string name);
		Task<TopListStats> GetCityNameStats(string city);
		Task<DashboardStatsForHome> GetDashboardStatsForHome();
		Task<PagedResult<ItemStats>> GetNameAnalysis(
			string? type,
			int? syllables,
			int? lengthFrom,
			int? lengthTo,
			string? startsWith,
			int page,
			int pageSize);
	}
}
