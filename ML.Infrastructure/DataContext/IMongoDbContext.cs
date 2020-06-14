using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Infrastructure.DataContext
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }
}
