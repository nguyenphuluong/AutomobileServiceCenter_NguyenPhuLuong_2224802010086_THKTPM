using ASC.DataAccess.Interfaces;
using ASC.Model.BaseTypes;
using Microsoft.EntityFrameworkCore;

namespace ASC.DataAccess
{
    public class Repository<T> : IRepository<T> where T : BaseEntity, new()
    {
        private readonly DbContext _dbContext;

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(T entity)
        {
            var entry = entity as BaseEntity;
            entry.CreatedDate = DateTime.UtcNow;

            await _dbContext.Set<T>().AddAsync(entity);
        }

        public void Update(T entity)
        {
            var entry = entity as BaseEntity;
            entry.UpdatedDate = DateTime.UtcNow;

            _dbContext.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            var entry = entity as BaseEntity;
            entry.IsDeleted = true;
            entry.UpdatedDate = DateTime.UtcNow;

            _dbContext.Set<T>().Update(entity);
        }

        public async Task<T?> FindAsync(string partitionKey, string rowKey)
        {
            return await _dbContext.Set<T>().FindAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<T>> FindAllByPartitionKeyAsync(string partitionKey)
        {
            var result = await _dbContext.Set<T>()
                .Where(x => x.PartitionKey == partitionKey)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<T>> FindAllAsync()
        {
            var result = await _dbContext.Set<T>().ToListAsync();
            return result;
        }
    }
}