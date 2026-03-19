using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnomasticsDataSystem.Core.Interfaces
{
	public interface IStatisticsService
	{
		//Task<int> GetUniqueNamesCount();
		//Task<int> GetUniqueSurnamesCount();
		//Task<int> GetTotalPeopleCount();
		//Task<int> GetUniqueCitiesCount();
		//Task<List<Person>> GetPersonsPage(int page, int pageSize);
		//Task<List<Person>> GetPersonsPageFiltered(
		//	int page,
		//	int pageSize,
		//	string city,
		//	string name,
		//	string lastName,
		//	int? yearFrom,
		//	int? yearTo
		//);

		//Task<int> GetFilteredPeopleCount(
		//	string city,
		//	string name,
		//	string lastName,
		//	int? yearFrom,
		//	int? yearTo
		//);
		Task<PagedResult<Person>> GetPersonsPage(
			int page,
			int pageSize,
			string? city,
			string? name,
			string? lastName,
			int? yearFrom,
			int? yearTo);

		//Task<List<TopItem>> GetTopNames(int limit = 5);
		//Task<List<TopItem>> GetTopSurnames(int limit = 5);
		//Task<List<TopItem>> GetTopCities(int limit = 5);
		Task<PagedResult<ItemStats>> GetNameStats(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		//Task<int> GetNameStatsCount(string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		Task<PagedResult<ItemStats>> GetSurnameStats(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		Task<PagedResult<ItemStats>> GetCityStats(int page, int pageSize, string? citySearch, int? minCount, int? maxCount);
		//Task<int> GetSurnameStatsCount(string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		Task<TopListStats> GetNameCityStats(string name);
		Task<TopListStats> GetSurnameCityStats(string name);
		Task<TopListStats> GetCityNameStats(string city);
		Task<DashboardStatsForHome> GetDashboardStatsForHome();
	}
}
