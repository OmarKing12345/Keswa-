using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kesawa_Data_Access.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T?> CreateAsync(T entity);

        T? Update(T entity);

        bool Delete(T entity);

        Task<T?> GetOneAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true);

        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true);

        Task<bool> CommitAsync();
    }
}
