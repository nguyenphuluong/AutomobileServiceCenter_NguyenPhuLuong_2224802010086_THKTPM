using ASC.DataAccess.Interfaces;
using ASC.Model.BaseTypes;
using Microsoft.EntityFrameworkCore;

namespace ASC.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private Dictionary<string, object> _repositories;

        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int CommitTransaction()
        {
            return _dbContext.SaveChanges();
        }

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            if (_repositories == null)
                _repositories = new Dictionary<string, object>();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);
                var repositoryInstance = Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(T)),
                    _dbContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}