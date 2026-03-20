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


		public async Task<PagedResult<ItemStats>> GetNameStats(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith)
		{
			return await _personRepository.GetNameStatsAsync(page, pageSize, nameSearch, minCount, maxCount, startsWith);
		}

		public async Task<PagedResult<ItemStats>> GetSurnameStats(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith)
		{
			return await _personRepository.GetSurnameStatsAsync(page, pageSize, nameSearch, minCount, maxCount, startsWith);
		}
		public async Task<PagedResult<ItemStats>> GetCityStats(int page, int pageSize, string? citySearch, int? minCount, int? maxCount) { 
			return await _personRepository.GetCityStatsAsync(page, pageSize, citySearch, minCount, maxCount);
		}
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

		public async Task<PagedResult<ItemStats>> GetNameAnalysis(
			string? type,
			int? syllables,
			int? lengthFrom,
			int? lengthTo,
			string? startsWith,
			int page,
			int pageSize)
		{
			return await _personRepository.GetNameAnalysisAsync(type, syllables, lengthFrom, lengthTo, startsWith, page, pageSize);

		}

	}
}
