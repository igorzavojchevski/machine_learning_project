using ML.Domain.Entities.Mongo.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ML.Infrastructure.Interfaces
{
    public interface IMongoBaseRepository<T> : IDisposable where T : MongoBaseEntity
    {
        //Sync
        long CountDocuments(Expression<Func<T, bool>> expression);
        T GetSingle(Expression<Func<T, bool>> expression);
        IQueryable<T> GetAll();
        IEnumerable<T> GetAll(Expression<Func<T, bool>> expression);
        IEnumerable<T> GetAll(int page, int pageSize);
        IEnumerable<T> GetAll(Expression<Func<T, bool>> expression, int page, int pageSize);

        void InsertOne(T item);
        void InsertMany(IEnumerable<T> items);

        //bool UpdateOne(ObjectId id, Expression<Func<T, object>> expression, object updateFieldValue);
        bool Update(Expression<Func<T, bool>> expression, Expression<Func<T, object>> updateField, object updateFieldValue);
        bool Update(T item);

        void DeleteOne(Expression<Func<T, bool>> expression);
        void DeleteMany(Expression<Func<T, bool>> expression);

        //Async
        Task<T> GetSingleAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetAllAsync(int page, int pageSize);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression, int page, int pageSize);

        Task InsertOneAsync(T item);
        Task InsertManyAsync(IEnumerable<T> items);

        Task DeleteOneAsync(Expression<Func<T, bool>> expression);
        Task DeleteManyAsync(Expression<Func<T, bool>> expression);
    }
}
