using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace Shared.Repositories
{
    public interface IRepository<TEntity, TId> where TEntity : class
    {
        IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        TEntity GetByID(TId id);
        void Insert(TEntity entity);
        void Delete(TId id);
        void Delete(params TId[] id);
        void Delete(TEntity entityToDelete);
        void Update(TEntity entityToUpdate);
        Task SaveChangesAsync();
        List<T> ToSql<T>(FormattableString query);
    }
}