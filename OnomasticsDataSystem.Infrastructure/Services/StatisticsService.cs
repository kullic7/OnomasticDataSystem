using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Models;
using OnomasticsDataSystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnomasticsDataSystem.Infrastructure.Services
{
	public class StatisticsService : IStatisticsService
	{
		private readonly IPersonRepository _personRepository;
		private DashboardStatsForHome? _cache;
		private DateTime _lastUpdate;

		public StatisticsService(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		//public async Task<int> GetUniqueNamesCount()
		//{
		//	return await _personRepository.GetUniqueNamesCountAsync();
		//}

		//public async Task<int> GetUniqueSurnamesCount()
		//{
		//	return await _personRepository.GetUniqueSurnamesCountAsync();
		//}
		//public async Task<int> GetTotalPeopleCount()
		//{
		//	return await _personRepository.GetTotalPeopleCountAsync();
		//}
		//public async Task<int> GetUniqueCitiesCount()
		//{
		//	return await _personRepository.GetUniqueCitiesCountAsync();
		//}
		//public async Task<List<Person>> GetPersonsPage(int page, int pageSize)
		//{
		//	return await _personRepository.GetPagedAsync(page, pageSize);
		//}
		//public async Task<List<Person>> GetPersonsPageFiltered(
		//	int page,
		//	int pageSize,
		//	string city,
		//	string name,
		//	string lastName,
		//	int? yearFrom,
		//	int? yearTo)
		//{
		//	return await _personRepository.GetPersonsPageFilteredAsync(
		//		page, pageSize, city, name, lastName, yearFrom, yearTo);
		//}
		public async Task<PagedResult<Person>> GetPersonsPage(
			int page,
			int pageSize,
			string? city,
			string? name,
			string? lastName,
			int? yearFrom,
			int? yearTo)
		{
			return await _personRepository.GetPersonsPageAsync(
				page, pageSize,
				city, name, lastName,
				yearFrom, yearTo);
		}
		//public async Task<int> GetFilteredPeopleCount(
		//	string city,
		//	string name,
		//	string lastName,
		//	int? yearFrom,
		//	int? yearTo)
		//{
		//	return await _personRepository.GetFilteredPeopleCountAsync(
		//		city, name, lastName, yearFrom, yearTo);
		//}

		//public async Task<List<TopItem>> GetTopNames(int limit = 5)
		//{
		//	return await _personRepository.GetTopNamesAsync(limit);
		//}

		//public async Task<List<TopItem>> GetTopSurnames(int limit = 5)
		//{
		//	return await _personRepository.GetTopSurnamesAsync(limit);
		//}

		//public async Task<List<TopItem>> GetTopCities(int limit = 5)
		//{
		//	return await _personRepository.GetTopCitiesAsync(limit);
		//}

		public async Task<PagedResult<ItemStats>> GetNameStats(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith)
		{
			return await _personRepository.GetNameStatsAsync(page, pageSize, nameSearch, minCount, maxCount, startsWith);
		}
		//public async Task<int> GetNameStatsCount(string? nameSearch, int? minCount, int? maxCount, string? startsWith)
		//{
		//	return await _personRepository.GetNameStatsCountAsync(nameSearch, minCount, maxCount, startsWith);
		//}
		public async Task<PagedResult<ItemStats>> GetSurnameStats(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith)
		{
			return await _personRepository.GetSurnameStatsAsync(page, pageSize, nameSearch, minCount, maxCount, startsWith);
		}
		public async Task<PagedResult<ItemStats>> GetCityStats(int page, int pageSize, string? citySearch, int? minCount, int? maxCount) { 
			return await _personRepository.GetCityStatsAsync(page, pageSize, citySearch, minCount, maxCount);
		}
		//public async Task<int> GetSurnameStatsCount(string? nameSearch, int? minCount, int? maxCount, string? startsWith)
		//{
		//	return await _personRepository.GetSurnameStatsCountAsync(nameSearch, minCount, maxCount, startsWith);
		//}
		public async Task<TopListStats> GetNameCityStats(string name)
		{
			return await _personRepository.GetNameCityStatsAsync(name);
		}
		public async Task<TopListStats> GetSurnameCityStats(string name)
		{
			return await _personRepository.GetSurnameCityStatsAsync(name);
		}
		public async Task<TopListStats> GetCityNameStats(string city) 
		{
			return await _personRepository.GetCityNameStatsAsync(city);
		}


		public async Task<DashboardStatsForHome> GetDashboardStatsForHome()
		{
			if (_cache != null && (DateTime.UtcNow - _lastUpdate).TotalMinutes < 60)
				return _cache;

			var stats = new DashboardStatsForHome
			{
				TotalPersons = await _personRepository.GetTotalPeopleCountAsync(),
				UniqueNames = await _personRepository.GetUniqueNamesCountAsync(),
				UniqueSurnames = await _personRepository.GetUniqueSurnamesCountAsync(),
				UniqueCities = await _personRepository.GetUniqueCitiesCountAsync(),
				TopNames = await _personRepository.GetTopNamesAsync(5),
				TopSurnames = await _personRepository.GetTopSurnamesAsync(5),
				TopCities = await _personRepository.GetTopCitiesAsync(5)
			};

			_cache = stats;
			_lastUpdate = DateTime.UtcNow;

			return stats;
		}
	}
}
