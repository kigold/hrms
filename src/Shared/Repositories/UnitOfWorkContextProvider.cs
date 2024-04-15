using Microsoft.EntityFrameworkCore;

namespace Shared.Repositories
{
    public interface IDbContextProvider<out TDbContext>
           where TDbContext : DbContext
    {
        TDbContext GetDbContext();
    }

    public sealed class UnitOfWorkDbContextProvider<TDbContext> : IDbContextProvider<TDbContext>
      where TDbContext : DbContext
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnitOfWorkDbContextProvider(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public TDbContext GetDbContext()
        {
            return _unitOfWork.GetDbContext<TDbContext>();
        }
    }

    public static class EfUnitOfWorkExtensions
    {
        public static TDbContext GetDbContext<TDbContext>(this IUnitOfWork unitOfWork)
           where TDbContext : DbContext
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            if (!(unitOfWork is UnitOfWork))
            {
                throw new ArgumentException("unitOfWork is not type of " + typeof(UnitOfWork).FullName, nameof(unitOfWork));
            }

            return (unitOfWork as UnitOfWork).GetOrCreateDbContext<TDbContext>();
        }
    }
}
