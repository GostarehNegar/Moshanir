using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Helpers;
using System.Threading;

namespace GN.Library.Messaging.Streams.LiteDb
{


    public class LiteDbStream : IStream
    {
        private LiteDB.LiteDatabase db;
        private LiteDB.ILiteCollection<LiteDbEventData> collection;
        private string connectionString;
        public LiteDbStream(string connectionString)
        {
            if (!connectionString.Contains("="))
            {
                connectionString = $"Filename={connectionString}";
            }
            this.connectionString = connectionString;
        }
        private LiteDB.LiteDatabase GetDatabase()
        {
            if (this.db == null)
            {
                this.db = new LiteDB.LiteDatabase(this.connectionString);
            }

            return this.db;
        }
        private LiteDB.ILiteCollection<LiteDbEventData> GetCollection()
        {
            if (this.collection == null)
            {
                this.collection = this.GetDatabase().GetCollection<LiteDbEventData>();
            }
            return this.collection;
        }
        private static Dictionary<string, string> Parse(string connectionString)
        {
            var result = new Dictionary<string, string>();
            foreach (var part in connectionString.Split(';'))
            {
                var pair = part.Split('=');
                result.Add(pair[0], pair[1]);
            }
            if (!result.ContainsKey("Connection"))
                result.Add("Connection", "direct");
            if (!result.ContainsKey("ReadOnly"))
                result.Add("ReadOnly", "false");
            return result;
        }
        private static string ToConnectionString(Dictionary<string, string> props)
        {
            var result = "";
            foreach (var item in props)
            {

                if (result.Length > 0)
                    result = result + ";";
                result = result + $"{item.Key}={item.Value}";
            }
            return result;
        }

        private async Task<LiteDB.LiteDatabase> Lock(bool write, CancellationToken cancellationToken)
        {
            var connection = Parse(this.connectionString);
            connection["ReadOnly"] = write ? "false" : "true";
            var str = ToConnectionString(connection);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    return new LiteDB.LiteDatabase(str);
                }
                catch { }
                await Task.Delay(100);
            }
            return null;

        }


        public void Dispose()
        {
            this.collection = null;
            this.db?.Dispose();
            this.db = null;
        }


        public Task<IStream> OpenAsync()
        {
            return Task.FromResult<IStream>(this);
        }


        public Task<IEnumerable<MessagePack>> SaveAsync(IEnumerable<MessagePack> events, long? expectedVersion = null, CancellationToken cancellationToken = default)
        {

            return Task.Run<IEnumerable<MessagePack>>(async () =>
                  {
                      using (var db = await this.Lock(true, cancellationToken))
                      {
                          var pos = db.GetCollection<LiteDbEventData>().LongCount();
                          db.GetCollection<LiteDbEventData>()
                            .InsertBulk(
                              events.Select(x => LiteDbEventData.FromMessagePack(x)));// new LiteDbEventData { Name = x.Subject, Payload = x.Payload, Timestamp = x.Timestamp }));
                          return db.GetCollection<LiteDbEventData>()
                            .Find(x => x.Id > pos)
                            .ToArray()
                            .Select(x => x.ToMessagePack()) //  new MessagePack(x.Id, x.Payload, x.Name, x.Timestamp))
                            .ToArray();
                      }

                  });

        }

        public Task Replay(Func<MessagePack, Task<bool>> callBack, long? pos = null, CancellationToken cancellationToken = default)
        {
            pos = (pos ?? 0) + 1;
            var chunk_size = 1000;
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    MessagePack[] items = new MessagePack[] { };
                    using (var db = await this.Lock(false, cancellationToken))
                    {
                        items = db.GetCollection<LiteDbEventData>()
                            .Find(x => x.Id >= pos && x.Id < (pos + chunk_size))
                            //.OrderByDescending(x => x.Id)
                            .ToArray()
                            .Select(x => x.ToMessagePack())// new MessagePack(x.Id, x.Payload, x.Name, x.Timestamp))
                            .ToArray();
                        //}
                        if (items.Length == 0)
                            return;
                        foreach (var item in items)
                        {
                            if (!await callBack(item))
                                return;
                        }
                        pos = pos + chunk_size;
                    }
                }
            });
        }

        public Task ReplayEx(Func<ReplayContext, Task> callBack, long? pos = null, int? chunkSize = 1000, CancellationToken cancellationToken = default)
        {
            pos = (pos ?? 0) + 1;
            int chunk_size = chunkSize ?? 1000;
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var context = new ReplayContext();
                    try
                    {
                        using (var db = await this.Lock(true, cancellationToken))
                        {
                            var count = db.GetCollection<LiteDbEventData>().LongCount();

                            MessagePack[] items = new MessagePack[] { };
                            items = db.GetCollection<LiteDbEventData>()
                                .Find(x => x.Id >= pos && x.Id < (pos + chunk_size))
                                .ToArray()
                                .Select(x => x.ToMessagePack())// new MessagePack(x.Id, x.Payload, x.Name, x.Timestamp))
                                .ToArray();
                            context.Events = items;
                            context.Remaining = count - (pos.Value + items.Length) + 1;
                            context.Position = pos.Value + items.Length;
                            context.TotalCount = count;
                        }
                        await callBack(context);
                        if (context.Stop || context.Events.Length == 0)
                            return;
                        pos = pos + chunk_size;
                    }
                    catch (Exception err)
                    {

                    }
                }
            });

        }
    }
}
