namespace RateLimiting.Services;
public interface IMongoService<T> where T : class
{
    Task<IEnumerable<T>> GetAsync();
    Task<T?> GetAsync(string id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(string id, T updatedEntity);
    Task RemoveAsync(string id);
}