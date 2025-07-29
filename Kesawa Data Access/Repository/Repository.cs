using Kesawa_Data_Access.Data;
using Kesawa_Data_Access.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kesawa_Data_Access.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // CRUD
        public async Task<T?> CreateAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);

                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");

                return null;
            }
        }

        public T? Update(T entity)
        {
            try
            {
                _dbSet.Update(entity);

                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");

                return null;
            }
        }

        public bool Delete(T entity)
        {
            try
            {
                _dbSet.Remove(entity);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");

                return false;
            }
        }

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            return (await GetAsync(expression, includes, tracked)).FirstOrDefault();
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            IQueryable<T> entities = _dbSet;

            // Filter
            if (expression is not null)
            {
                entities = entities.Where(expression);
            }

            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    entities = entities.Include(item);
                }
            }

            if (!tracked)
            {
                entities = entities.AsNoTracking();
            }

            return (await entities.ToListAsync());
        }

        public async Task<bool> CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");

                return false;
            }
        }
    }
}
