using GN.Library.Data;
using GN.Library.Data.Deprecated;
using GN.Library.Data.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
    public class EndPointData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsServer { get; set; }
        public DateTime? LastSeen { get; set; }


    }
    public interface IEndPointRepository : IDocumentRepository<EndPointData>
    {
        EndPointData EnsureEndpoint(string endpoint, bool? isServer);
        DateTime? GetLastSeen(string endpoint);
        void Touch(string endpoint);
        IEnumerable<EndPointData> GetCachedValues(bool refresh = false);
        void DeleteAll();
        EndPointData GetEndPoint(string endpoint, bool create = false, bool? isServer = null);
        IEnumerable<EndPointData> GetServers();
        Task CleanUp(bool force = false);
    }
    class EndPointRepository : DocumentRepository<EndPointData>, IEndPointRepository
    {
        private ConcurrentDictionary<string, EndPointData> cache;
        public EndPointRepository(MessagingConfig config) : base("")
        {
            this.connectionString = new DocumentStoreConnectionString
            {
                FileName = MessagingConstants.Instance.GetMessagingDbFileName(config.EndPointName)
            };
        }
        public EndPointRepository(LiteDB.LiteDatabase db) : base(db) { }

        private ConcurrentDictionary<string, EndPointData> GetEndpoints(bool refersh = false)
        {
            if (cache == null || refersh || cache.Count != this.Count(null))
            {
                this.cache = new ConcurrentDictionary<string, EndPointData>(this.GetAll().Select(x => new KeyValuePair<string, EndPointData>(x.Id, x)));
            }
            return this.cache;
        }
        public EndPointData GetEndPoint(string endpoint, bool create = false, bool? isServer = null)
        {
            EndPointData result = null;
            if (this.GetEndpoints().TryGetValue(endpoint, out result))
            {

            }
            if (create && (result == null || (isServer.HasValue && result.IsServer != isServer.Value)))
            {
                result = EnsureEndpoint(endpoint, isServer);
                this.GetEndpoints().TryGetValue(endpoint, out result);
            }
            return result;
        }
        public EndPointData EnsureEndpoint(string endpoint, bool? isServer)
        {
            var existing = this.Get(endpoint);
            if (existing == null)
            {
                existing = this.Upsert(new EndPointData
                {
                    Id = endpoint,
                    Name = endpoint,
                    IsServer = isServer ?? false,
                    LastSeen = null
                });
                this.cache = null;
            }
            else if (isServer.HasValue && existing.IsServer != isServer.Value)
            {
                existing = this.Upsert(new EndPointData
                {
                    Id = endpoint,
                    Name = endpoint,
                    IsServer = isServer.Value,
                    LastSeen = null
                });
                this.cache = null;
            }
            return existing;
        }
        public DateTime? GetLastSeen(string endpoint)
        {
            return this.GetEndpoints().TryGetValue(endpoint, out var result)
                ? result.LastSeen
                : (DateTime?)null;
        }
        public void Touch(string endpoint)
        {
            var e = this.GetEndPoint(endpoint, true, null);
            if (e != null)
                e.LastSeen = DateTime.Now;
        }

        public IEnumerable<EndPointData> GetCachedValues(bool refresh = false)
        {
            return this.GetEndpoints().Values;
        }

        public void DeleteAll()
        {
            this.GetCollection().Delete(x => true);
        }

        public IEnumerable<EndPointData> GetServers()
        {
            return this.GetEndpoints().Values.Where(x => x.IsServer);
        }

        public Task CleanUp(bool force = false)
        {
            return Task.CompletedTask;
        }
    }
}
