using OnomasticsDataSystem.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnomasticsDataSystem.Core.Interfaces
{
	public interface ISourceRepository
	{
		Task AddAsync(Source source);
		Task<List<Source>> GetAllAsync();
		Task<Source?> GetByNameAsync(string name);
		Task SaveChangesAsync();
	}
}
