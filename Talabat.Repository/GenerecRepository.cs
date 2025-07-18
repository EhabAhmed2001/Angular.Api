﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Repository;
using Talabat.Core.Specifications;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly StoreContext _dbContext;

        public GenericRepository(StoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Without Specification
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            if (typeof(T) == typeof(Product))
                return (IEnumerable<T>)await _dbContext.Products.Include(P => P.Brand).Include(P => P.Type).ToListAsync();

            return await _dbContext.Set<T>().ToListAsync();
        }


        public async Task<T> GetByIdAsync(int id, List<Expression<Func<T, object>>>? Includes = null)
        {
            if (Includes != null)
            {
                var query = _dbContext.Set<T>().AsQueryable();
                foreach (var include in Includes)
                {
                    query = query.Include(include);
                }
                return await query.FirstOrDefaultAsync(P => P.Id == id);
            }
            return await _dbContext.Set<T>().FindAsync(id);
        }

        #endregion

        #region With Specification
        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> Spec)
        {
            return await GenerateSpec(Spec).ToListAsync();
        }

        public async Task<T> GetByIdWithSpecAsync(ISpecifications<T> Spec)
        {
            return await GenerateSpec(Spec).FirstOrDefaultAsync();
        }

        #endregion

        private IQueryable<T> GenerateSpec(ISpecifications<T> Spec)
        {
            return SpecificationEvalutor<T>.GetQuery(_dbContext.Set<T>(), Spec).Result;
        }

        public async Task AddAsync(T item)
        => await _dbContext.Set<T>().AddAsync(item);

        public void Delete(T item)
            => _dbContext.Set<T>().Remove(item);

    }
}
