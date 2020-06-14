using ML.BL.Mongo.Interfaces.Base;
using ML.Domain.Entities.Mongo.Base;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ML.BL.Mongo.Concrete.Base
{
    public abstract class MongoBaseService<T,R> : IMongoBaseService<T> where R : IMongoBaseRepository<T> where T : MongoBaseEntity
    {
        public virtual R Repository { get; protected set; }

        #region Count
        public long CountDocuments(Expression<Func<T, bool>> expression)
        {
            return Repository.CountDocuments(expression);
        }
        #endregion

        #region GetSingle
        public virtual T GetSingle(Expression<Func<T, bool>> expression)
        {
            return Repository.GetSingle(expression);
        }

        public virtual Task<T> GetSingleAsync(Expression<Func<T, bool>> expression)
        {
            return Repository.GetSingleAsync(expression);
        }
        #endregion


        #region GetAll
        public virtual IQueryable<T> GetAll()
        {
            return Repository.GetAll();
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> expression)
        {
            return Repository.GetAll(expression);
        }

        public virtual IEnumerable<T> GetAll(int page, int pageSize)
        {
            return Repository.GetAll(page, pageSize);
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> expression, int page, int pageSize)
        {
            return Repository.GetAll(expression, page, pageSize);
        }

        public virtual Task<IEnumerable<T>> GetAllAsync()
        {
            return Repository.GetAllAsync();
        }

        public virtual Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression)
        {
            return Repository.GetAllAsync(expression);
        }

        public virtual Task<IEnumerable<T>> GetAllAsync(int page, int pageSize)
        {
            return Repository.GetAllAsync(page, pageSize);
        }

        public virtual Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression, int page, int pageSize)
        {
            return Repository.GetAllAsync(expression, page, pageSize);
        }
        #endregion


        #region Insert
        public virtual void InsertOne(T item)
        {
            Repository.InsertOne(item);
        }
        public virtual void InsertMany(IEnumerable<T> items)
        {
            Repository.InsertMany(items);
        }

        public virtual Task InsertManyAsync(IEnumerable<T> items)
        {
            return Repository.InsertManyAsync(items);
        }

        public virtual Task InsertOneAsync(T item)
        {
            return Repository.InsertOneAsync(item);
        }
        #endregion

        #region Update
        public virtual bool Update(Expression<Func<T, bool>> expression, Expression<Func<T, object>> updateField, object updateFieldValue)
        {
            return Repository.Update(expression, updateField, updateFieldValue);
        }

        public virtual bool Update(T item)
        {
            return Repository.Update(item);
        }
        #endregion


        #region Delete
        public virtual void DeleteOne(Expression<Func<T, bool>> expression)
        {
            Repository.DeleteOne(expression);
        }

        public void DeleteMany(Expression<Func<T, bool>> expression)
        {
            Repository.DeleteMany(expression);
        }

        public virtual Task DeleteOneAsync(Expression<Func<T, bool>> expression)
        {
            return Repository.DeleteOneAsync(expression);
        }

        public virtual Task DeleteManyAsync(Expression<Func<T, bool>> expression)
        {
            return Repository.DeleteManyAsync(expression);
        }
        #endregion
    }
}
