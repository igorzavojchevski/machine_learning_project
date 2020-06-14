using ML.Domain.Entities.Mongo.Base;
using ML.Infrastructure.DataContext;
using ML.Infrastructure.Helpers;
using ML.Infrastructure.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ML.Infrastructure.Repositories
{
    public class MongoBaseRepository<T> : IMongoBaseRepository<T> where T : MongoBaseEntity
    {
        protected readonly IMongoDbContext _mongoDbContext;
        protected IMongoCollection<T> _dbCollection;

        protected MongoBaseRepository(IMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
            _dbCollection = _mongoDbContext.GetCollection<T>(Pluralization.Pluralize(typeof(T).Name));
        }

        #region Count
        public long CountDocuments(Expression<Func<T, bool>> expression)
        {
            return _dbCollection.CountDocuments(expression);
        }
        #endregion

        #region FindSingle
        public T GetSingle(Expression<Func<T, bool>> expression)
        {
            return _dbCollection.Find(expression).SingleOrDefault();
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbCollection.Find(expression).SingleOrDefaultAsync();
        }
        #endregion

        #region GetAll
        public IQueryable<T> GetAll()
        {
            return _dbCollection.AsQueryable();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> expression)
        {
            return _dbCollection.Find(expression).ToEnumerable();
        }

        public IEnumerable<T> GetAll(int page, int pageSize)
        {
            return PagingExtension.DoPaging(_dbCollection.Find<T>(null), page, pageSize);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> expression, int page, int pageSize)
        {
            return PagingExtension.DoPaging(_dbCollection.Find(expression), page, pageSize);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbCollection.AsQueryable().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbCollection.Find(expression).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(int page, int pageSize)
        {
            return PagingExtension.DoPaging(await _dbCollection.Find(null).ToListAsync(), page, pageSize);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression, int page, int pageSize)
        {
            return PagingExtension.DoPaging(await _dbCollection.Find(expression).ToListAsync(), page, pageSize);
        }
        #endregion

        #region Insert
        public void InsertOne(T item)
        {
            _dbCollection.InsertOne(item);
        }

        public async Task InsertOneAsync(T item)
        {
            await _dbCollection.InsertOneAsync(item);
        }

        public void InsertMany(IEnumerable<T> items)
        {
            _dbCollection.InsertMany(items);
        }

        public async Task InsertManyAsync(IEnumerable<T> items)
        {
            await _dbCollection.InsertManyAsync(items);
        }
        #endregion

        #region Update
        public bool Update(Expression<Func<T, bool>> expression, Expression<Func<T, object>> updateField, object updateFieldValue)
        {
            UpdateResult result = _dbCollection.UpdateOne(Builders<T>.Filter.Where(expression), Builders<T>.Update.Set(updateField, updateFieldValue));
            return result != null && result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public bool Update(T item)
        {
            ReplaceOneResult result = _dbCollection.ReplaceOne(t => t.Id == item.Id, item);
            return result != null && result.IsAcknowledged && result.ModifiedCount > 0;
        }
        #endregion

        #region Delete
        public void DeleteOne(Expression<Func<T, bool>> expression)
        {
            _dbCollection.DeleteOne<T>(expression);
        }

        public void DeleteMany(Expression<Func<T, bool>> expression)
        {
            _dbCollection.DeleteMany<T>(expression);
        }

        public async Task DeleteOneAsync(Expression<Func<T, bool>> expression)
        {
            await _dbCollection.DeleteOneAsync(expression);
        }

        public async Task DeleteManyAsync(Expression<Func<T, bool>> expression)
        {
            await _dbCollection.DeleteManyAsync(expression);
        }
        #endregion


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MongoBaseRepository() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
