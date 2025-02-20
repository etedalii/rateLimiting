using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RateLimiting.Services;

namespace RateLimiting.Data;
public abstract class MongoService<T> : IMongoService<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public MongoService(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _collection = mongoDatabase.GetCollection<T>(typeof(T).Name);
    }

    public virtual async Task<IEnumerable<T>> GetAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public virtual async Task<T?> GetAsync(string id)
    {
        var filter = ObjectId.TryParse(id, out var objectId)
            ? Builders<T>.Filter.Eq("_id", objectId)
            : Builders<T>.Filter.Eq("_id", id);

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

public virtual async Task<T> CreateAsync(T entity)
{
    await _collection.InsertOneAsync(entity);
    return entity; // Return the inserted entity
}

public virtual async Task<T> UpdateAsync(string id, T updatedEntity)
{
    var filter = ObjectId.TryParse(id, out var objectId)
        ? Builders<T>.Filter.Eq("_id", objectId)
        : Builders<T>.Filter.Eq("_id", id);

    var result = await _collection.ReplaceOneAsync(filter, updatedEntity);

    if (result.MatchedCount == 0)
    {
        throw new KeyNotFoundException($"No document found with id: {id}");
    }

    return await _collection.Find(filter).FirstOrDefaultAsync(); // Return the updated document
}

    public virtual async Task RemoveAsync(string id)
    {
        var filter = ObjectId.TryParse(id, out var objectId)
            ? Builders<T>.Filter.Eq("_id", objectId)
            : Builders<T>.Filter.Eq("_id", id);

        var result = await _collection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            throw new KeyNotFoundException($"No document found with id: {id}");
        }
    }
}