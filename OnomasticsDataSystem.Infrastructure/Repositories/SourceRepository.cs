using Microsoft.EntityFrameworkCore;
using OnomasticsDataSystem.Core.Entity;
using OnomasticsDataSystem.Core.Interfaces;
using OnomasticsDataSystem.Infrastructure.Data;

namespace OnomasticsDataSystem.Infrastructure.Repositories
{
	public class SourceRepository : ISourceRepository
	{
		private readonly ApplicationDbContext _context;

		public SourceRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Source source)
		{
			await _context.Sources.AddAsync(source);
		}

		public async Task<List<Source>> GetAllAsync()
		{
			return await _context.Sources
				.AsNoTracking()
				.OrderBy(s => s.Name)
				.ToListAsync();
		}

		public async Task<Source?> GetByNameAsync(string name)
		{
			return await _context.Sources
				.FirstOrDefaultAsync(s => s.Name == name);
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}