using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ML.Infrastructure.DataContext
{
    public class MongoDbContext : IMongoDbContext
    {
        private IConfiguration Configuration { get; }

        private IMongoDatabase _db = null;
        private MongoClient _mongoClient { get; set; }

        public MongoDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
            _mongoClient = Initialize();
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _db.GetCollection<T>(name);
        }

        private MongoClient Initialize()
        {
            try
            {
                string connectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString"); //Configuration["ConnectionString:MongoDBConnectionString"];
                if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString), "Cannot be null or empty !");

                MongoClient client = new MongoClient(connectionString);

                string _databaseName = MongoUrl.Create(connectionString).DatabaseName;
                if (string.IsNullOrWhiteSpace(_databaseName)) throw new ArgumentNullException(nameof(_databaseName), "Cannot be null or empty !");
                _db = client.GetDatabase(_databaseName);

                _db.RunCommand<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));

                if (client.Cluster.Description.State == MongoDB.Driver.Core.Clusters.ClusterState.Disconnected) throw new MongoConnectionException(new MongoDB.Driver.Core.Connections.ConnectionId(client.Cluster.Description.Servers.FirstOrDefault()?.ServerId), "Connection state - Disconnected");

                return client;
            }
            catch (Exception ex)
            {
                throw new Exception("MongoDB connection failed.", ex);
            }
        }
    }
}
