using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Models;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace OnomasticsDataSystem.Core.Interfaces
{
	public interface IPersonRepository
	{
		Task AddAsync(Person person);
		Task AddRangeAsync(IEnumerable<Person> people);
		Task<IEnumerable<Person>> GetAllAsync();
		Task<Person?> FindByNameAsync(string firstName, string lastName);
		Task UpdateAsync(Person person);
		Task DeleteAsync(long id);
		void ClearTracker();
		Task<Person?> GetByIdAsync(long id);
		//Task SaveChangesAsync();

		Task<bool> ExistsAsync(string firstName, string lastName, int? birthYear, string birthCity);




		//services
		Task<PagedResult<ItemStats>> GetNameStatsAsync(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		//Task<int> GetNameStatsCountAsync(string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		Task<PagedResult<ItemStats>> GetSurnameStatsAsync(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		//Task<int> GetSurnameStatsCountAsync(string? nameSearch, int? minCount, int? maxCount, string? startsWith);
		Task<PagedResult<ItemStats>> GetCityStatsAsync(int page, int pageSize, string? citySearch, int? minCount, int? maxCount);
		Task<int> GetUniqueNamesCountAsync();
		Task<int> GetUniqueSurnamesCountAsync();
		Task<int> GetTotalPeopleCountAsync();
		Task<int> GetUniqueCitiesCountAsync();
		//Task<List<Person>> GetPagedAsync(int page, int pageSize);
		//Task<int> GetFilteredPeopleCountAsync(string city,
		//	string name,
		//	string lastName,
		//	int? yearFrom,
		//	int? yearTo);
		Task<PagedResult<Person>> GetPersonsPageAsync(
			int page,
			int pageSize,
			string? city,
			string? name,
			string? lastName,
			int? yearFrom,
			int? yearTo);


		Task<List<TopItem>> GetTopNamesAsync(int limit = 5);
		Task<List<TopItem>> GetTopSurnamesAsync(int limit = 5);
		Task<List<TopItem>> GetTopCitiesAsync(int limit = 5);
		Task<TopListStats> GetNameCityStatsAsync(string name);
		Task<TopListStats> GetSurnameCityStatsAsync(string name);
		Task<TopListStats> GetCityNameStatsAsync(string city);
	}
}
