using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Specifications;

namespace Talabat.Core.Repository
{
	public interface IGenericRepository<T> where T : BaseEntity
	{
		#region Without Specification

		Task<IEnumerable<T>> GetAllAsync();
		Task<T> GetByIdAsync(int id, List<Expression<Func<T, object>>>? Includes = null);

		#endregion

		#region With Specification

		Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> Spec);
		Task<T> GetByIdWithSpecAsync(ISpecifications<T> Spec);

		#endregion

		 Task AddAsync(T item);
		void Delete(T item);
    }
}
