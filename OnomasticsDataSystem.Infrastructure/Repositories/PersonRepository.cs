using Microsoft.EntityFrameworkCore;
using OnomasticsDataSystem.Common;
using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Interfaces;
using OnomasticsDataSystem.Core.Models;
using OnomasticsDataSystem.Infrastructure.Data;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace OnomasticsDataSystem.Infrastructure.Repositories
{
	public class PersonRepository : IPersonRepository
	{
		//pretože viac komponentov používa ten istý DbContext paralelne.
		private readonly IDbContextFactory<ApplicationDbContext> _factory;

		public PersonRepository(IDbContextFactory<ApplicationDbContext> factory)
		{
			_factory = factory;
		}


		public async Task<IEnumerable<Person>> GetAllAsync()
		{
			using var _db = _factory.CreateDbContext();
			return await _db.People.ToListAsync();
		}
		

		public async Task UpdateAsync(Person person)
		{
			using var _db = _factory.CreateDbContext();
			_db.People.Update(person);
			await Task.CompletedTask;
		}
		public void ClearTracker()
		{
			using var _db = _factory.CreateDbContext();
			_db.ChangeTracker.Clear();
		}
		public async Task<Person?> GetByIdAsync(long id)
		{
			using var _db = _factory.CreateDbContext();
			return await _db.People.FindAsync(id);
		}

		public async Task<Person?> FindByNameAsync(string firstName, string lastName)
		{
			using var _db = _factory.CreateDbContext();
			return await _db.People
				.FirstOrDefaultAsync(p => p.FirstName == firstName && p.LastName == lastName);
		}

		public async Task AddAsync(Person person)
		{
			using var _db = _factory.CreateDbContext();
			await _db.People.AddAsync(person);
		}

		public async Task AddRangeAsync(IEnumerable<Person> people)
		{
			using var db = _factory.CreateDbContext();

			await db.People.AddRangeAsync(people);
			await db.SaveChangesAsync();
		}

		public async Task DeleteAsync(long id)
		{
			using var _db = _factory.CreateDbContext();
			var person = await _db.People.FindAsync(id);
			if (person != null)
			{
				_db.People.Remove(person);
			}
		}

		//public async Task SaveChangesAsync()
		//{
		//	using var _db = _factory.CreateDbContext();
		//	await _db.SaveChangesAsync();
		//}

		public async Task<bool> ExistsAsync(string? firstName, string? lastName, int? birthYear, string? birthCity)
		{
			using var _db = _factory.CreateDbContext();
			return await _db.People.AnyAsync(p =>
				p.FirstName == firstName &&
				p.LastName == lastName &&
				p.BirthYear == birthYear &&
				p.BirthCity == birthCity);
		}


		//services
		//helper
		private IQueryable<Person> ApplyFilters(
			IQueryable<Person> query,
			string? city,
			string? name,
			string? lastName,
			int? yearFrom,
			int? yearTo)
		{
			if (!string.IsNullOrWhiteSpace(city))
			{
				var normalized = Helper.Normalize(city);
				query = query.Where(p => p.BirthCityNormalized!.StartsWith(normalized!));
			}

			if (!string.IsNullOrWhiteSpace(name))
			{
				var normalized = Helper.Normalize(name);
				query = query.Where(p => p.FirstNameNormalized!.StartsWith(normalized!));
			}

			if (!string.IsNullOrWhiteSpace(lastName))
			{
				var normalized = Helper.Normalize(lastName);
				query = query.Where(p => p.LastNameNormalized!.StartsWith(normalized!));
			}

			if (yearFrom.HasValue)
				query = query.Where(p => p.BirthYear >= yearFrom.Value);

			if (yearTo.HasValue)
				query = query.Where(p => p.BirthYear <= yearTo.Value);

			return query;
		}

		public async Task<PagedResult<ItemStats>> GetNameStatsAsync(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith)
		{
			using var db = _factory.CreateDbContext();

			var baseQuery = db.People
				.AsNoTracking();

			// filtrovanie na normalized
			if (!string.IsNullOrWhiteSpace(nameSearch))
			{
				var search = Helper.Normalize(nameSearch);
				baseQuery = baseQuery.Where(p => p.FirstNameNormalized!.Contains(search));
			}

			if (!string.IsNullOrWhiteSpace(startsWith))
			{
				var letter = Helper.Normalize(startsWith);
				baseQuery = baseQuery.Where(p => p.FirstNameNormalized!.StartsWith(letter));
			}

			var query = baseQuery
				.GroupBy(p => p.FirstNameNormalized)
				.Select(g => new ItemStats
				{
					Name = g.Select(x => x.FirstName)
		.First(),

					Count = g.Count()
				});

			if (minCount.HasValue)
				query = query.Where(x => x.Count >= minCount.Value);

			if (maxCount.HasValue)
				query = query.Where(x => x.Count <= maxCount.Value);

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(x => x.Count)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<ItemStats>
			{
				Items = items,
				TotalCount = totalCount
			};
			
		}

		
		public async Task<PagedResult<ItemStats>> GetSurnameStatsAsync(int page, int pageSize, string? nameSearch, int? minCount, int? maxCount, string? startsWith)
		{
			using var db = _factory.CreateDbContext();

			var baseQuery = db.People
				.AsNoTracking();

			// filtrovanie na normalized
			if (!string.IsNullOrWhiteSpace(nameSearch))
			{
				var search = Helper.Normalize(nameSearch);
				baseQuery = baseQuery.Where(p => p.LastNameNormalized!.Contains(search));
			}

			if (!string.IsNullOrWhiteSpace(startsWith))
			{
				var letter = Helper.Normalize(startsWith);
				baseQuery = baseQuery.Where(p => p.LastNameNormalized!.StartsWith(letter));
			}

			var query = baseQuery
				.GroupBy(p => p.LastNameNormalized)
				.Select(g => new ItemStats
				{
					Name = g
						.GroupBy(x => x.LastName)
						.OrderByDescending(x => x.Count())
						.Select(x => x.Key)
						.First(),

					Count = g.Count()
				});

			if (minCount.HasValue)
				query = query.Where(x => x.Count >= minCount.Value);

			if (maxCount.HasValue)
				query = query.Where(x => x.Count <= maxCount.Value);

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(x => x.Count)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<ItemStats>
			{
				Items = items,
				TotalCount = totalCount
			};
		}

		public async Task<PagedResult<ItemStats>> GetCityStatsAsync(
			int page,
			int pageSize,
			string? citySearch,
			int? minCount,
			int? maxCount)
		{
			using var db = _factory.CreateDbContext();

			var baseQuery = db.People
				.AsNoTracking()
				.Where(p => p.BirthCityNormalized != null);

			if (!string.IsNullOrWhiteSpace(citySearch))
			{
				var search = Helper.Normalize(citySearch);
				baseQuery = baseQuery.Where(p => p.BirthCityNormalized!.Contains(search));
			}

			var query = baseQuery
				.GroupBy(p => p.BirthCityNormalized)
				.Select(g => new ItemStats
				{
					Name = g.Min(x => x.BirthCity)!,
					Count = g.Count()
				});

			if (minCount.HasValue)
				query = query.Where(x => x.Count >= minCount.Value);

			if (maxCount.HasValue)
				query = query.Where(x => x.Count <= maxCount.Value);

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(x => x.Count)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<ItemStats>
			{
				Items = items,
				TotalCount = totalCount
			};
		}

		public async Task<int> GetUniqueNamesCountAsync()
		{
			using var _db = _factory.CreateDbContext();
			return await _db.People
				.Select(p => p.FirstNameNormalized)
				.Distinct()
				.CountAsync();
		}

		public async Task<int> GetUniqueSurnamesCountAsync()
		{
			using var _db = _factory.CreateDbContext();
			return await _db.People
				.Select(p => p.LastNameNormalized)
				.Distinct()
				.CountAsync();
		}

		public async Task<int> GetTotalPeopleCountAsync()
		{
			using var _db = _factory.CreateDbContext();
			return await _db.People.CountAsync();
		}

		public async Task<int> GetUniqueCitiesCountAsync()
		{
			using var _db = _factory.CreateDbContext();
			return await _db.People
				.Select(p => p.BirthCityNormalized)
				.Distinct()
				.CountAsync();
		}
		public async Task<PagedResult<Person>> GetPersonsPageAsync(
			int page,
			int pageSize,
			string? city,
			string? name,
			string? lastName,
			int? yearFrom,
			int? yearTo)
		{
			using var _db = _factory.CreateDbContext();

			var query = ApplyFilters(
				_db.People.AsQueryable(),
				city,
				name,
				lastName,
				yearFrom,
				yearTo);

			var totalCount = await query.CountAsync();
			//var totalCount = 0;
			var persons = await query
				.OrderBy(p => p.Id)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<Person>
			{
				Items = persons,
				TotalCount = totalCount
			};
		}

		public async Task<List<TopItem>> GetTopNamesAsync(int limit = 5)
		{
			using var db = _factory.CreateDbContext();

			var data = await db.People
				.Where(p => p.FirstNameNormalized != null)
				.GroupBy(p => p.FirstNameNormalized)
				.Select(g => new
				{
					Name = g
						.GroupBy(x => x.FirstName)
						.OrderByDescending(x => x.Count())
						.Select(x => x.Key)
						.First(),

					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.Take(limit)
				.ToListAsync();

			var max = data.Any() ? data.Max(x => x.Count) : 0;

			return data.Select((x, i) => new TopItem
			{
				Rank = i + 1,
				Name = x.Name!,
				Value = x.Count,
				Percent = max == 0 ? 0 : (double)x.Count / max * 100
			}).ToList();
		}
		public async Task<List<TopItem>> GetTopSurnamesAsync(int limit = 5)
		{
			using var db = _factory.CreateDbContext();

			var data = await db.People
				.Where(p => p.LastNameNormalized != null)
				.GroupBy(p => p.LastNameNormalized)
				.Select(g => new
				{
					Name = g
						.GroupBy(x => x.LastName)
						.OrderByDescending(x => x.Count())
						.Select(x => x.Key)
						.First(),

					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.Take(limit)
				.ToListAsync();

			var max = data.Max(x => x.Count);

			return data.Select((x, i) => new TopItem
			{
				Rank = i + 1,
				Name = x.Name!,
				Value = x.Count,
				Percent = (double)x.Count / max * 100
			}).ToList();
		}
		public async Task<List<TopItem>> GetTopCitiesAsync(int limit = 5)
		{
			using var db = _factory.CreateDbContext();

			var data = await db.People
				.Where(p => p.BirthCityNormalized != null && p.BirthCityNormalized != "")
				.GroupBy(p => p.BirthCityNormalized)
				.Select(g => new
				{
					City = g
						.GroupBy(x => x.BirthCity)
						.OrderByDescending(x => x.Count())
						.Select(x => x.Key)
						.First(),
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.Take(limit)
				.ToListAsync();

			var max = data.Max(x => x.Count);

			return data.Select((x, i) => new TopItem
			{
				Rank = i + 1,
				Name = x.City!,
				Value = x.Count,
				Percent = (double)x.Count / max * 100
			}).ToList();
		}
		public async Task<TopListStats> GetNameCityStatsAsync(string name)
		{
			using var db = _factory.CreateDbContext();
			var normalized = Helper.Normalize(name);
			var query = db.People
				.AsNoTracking()
				.Where(p => p.FirstNameNormalized == normalized);

			var displayName = await query
				.Select(p => p.FirstName)
				.FirstOrDefaultAsync();

			var data = await query
				.GroupBy(p => p.BirthCity)
				.Select(g => new
				{
					City = g.Key,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.Take(10)
				.ToListAsync();

			var max = data.Any() ? data.Max(x => x.Count) : 0;

			var items = data
				.Select((x, i) => new TopItem
				{
					Rank = i + 1,
					Name = x.City ?? "",
					Value = x.Count,
					Percent = max == 0 ? 0 : (double)x.Count / max * 100
				})
				.ToList();

			return new TopListStats
			{
				DisplayName = displayName ?? name,
				Items = items
			};
		}

		public async Task<TopListStats> GetSurnameCityStatsAsync(string name)
		{
			using var db = _factory.CreateDbContext();
			var normalized = Helper.Normalize(name);
			var query = db.People
				.AsNoTracking()
				.Where(p => p.LastNameNormalized == normalized);

			var displayName = await query
				.Select(p => p.LastName)
				.FirstOrDefaultAsync();

			var data = await query
				.GroupBy(p => p.BirthCity)
				.Select(g => new
				{
					City = g.Key,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.Take(10)
				.ToListAsync();

			var max = data.Any() ? data.Max(x => x.Count) : 0;

			var items = data
				.Select((x, i) => new TopItem
				{
					Rank = i + 1,
					Name = x.City ?? "",
					Value = x.Count,
					Percent = max == 0 ? 0 : (double)x.Count / max * 100
				})
				.ToList();

			return new TopListStats
			{
				DisplayName = displayName ?? name,
				Items = items
			};
		}
		public async Task<TopListStats> GetCityNameStatsAsync(string city)
		{
			using var db = _factory.CreateDbContext();

			var normalized = Helper.Normalize(city);

			var data = await db.People
				.AsNoTracking()
				.Where(p => p.BirthCityNormalized == normalized)
				.GroupBy(p => p.FirstNameNormalized)
				.Select(g => new
				{
					Name = g.GroupBy(x => x.FirstName)
							.OrderByDescending(x => x.Count())
							.Select(x => x.Key)
							.First(), // ✅ najčastejší tvar mena
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.Take(10)
				.ToListAsync();

			if (data.Count == 0)
				return new TopListStats();

			var max = data.Max(x => x.Count);

			var items = data.Select((x, i) => new TopItem
			{
				Rank = i + 1,
				Name = x.Name,
				Value = x.Count,
				Percent = (double)x.Count / max * 100
			}).ToList();

			return new TopListStats
			{
				DisplayName = city,
				Items = items
			};
		}
	}
}