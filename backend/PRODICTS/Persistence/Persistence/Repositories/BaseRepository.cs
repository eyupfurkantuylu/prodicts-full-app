using MongoDB.Driver;
using System.Linq.Expressions;
using Domain.Interfaces;
using Persistence.Context;

namespace Persistence.Repositories;

public class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public BaseRepository(MongoDbContext context, string collectionName)
    {
        _collection = context.GetType().GetProperty(collectionName)?.GetValue(context) as IMongoCollection<T> 
                     ?? throw new ArgumentException($"Collection {collectionName} not found");
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            // MongoDB ObjectId formatında arama yap
            var objectId = new MongoDB.Bson.ObjectId(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (FormatException)
        {
            // Geçersiz ObjectId formatı
            return null;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public virtual async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException("Entity must have an Id property");

        var id = idProperty.GetValue(entity)?.ToString();
        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("Entity Id cannot be null or empty");

        try
        {
            var objectId = new MongoDB.Bson.ObjectId(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            await _collection.ReplaceOneAsync(filter, entity);
            return entity;
        }
        catch (FormatException)
        {
            throw new InvalidOperationException($"Invalid ObjectId format: {id}");
        }
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var objectId = new MongoDB.Bson.ObjectId(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        try
        {
            var objectId = new MongoDB.Bson.ObjectId(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public virtual async Task<long> CountAsync()
    {
        return await _collection.CountDocumentsAsync(_ => true);
    }

    public virtual async Task<long> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.CountDocumentsAsync(predicate);
    }
}
