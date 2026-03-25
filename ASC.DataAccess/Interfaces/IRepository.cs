using ASC.Model.BaseTypes;

namespace ASC.DataAccess.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        Task<T?> FindAsync(string partitionKey, string rowKey);
        Task<IEnumerable<T>> FindAllByPartitionKeyAsync(string partitionKey);
        Task<IEnumerable<T>> FindAllAsync();
    }
}