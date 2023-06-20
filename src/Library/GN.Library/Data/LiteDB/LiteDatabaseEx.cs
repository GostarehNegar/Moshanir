using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Data.Lite
{
    public class LiteDatabaseEx
    {
        private readonly string connectionString;

        public class DisposableCollection<T> : IDisposable
        {
            public LiteDB.ILiteCollection<T> Collection { get; private set; }
            LiteDB.LiteDatabase db;

            internal DisposableCollection(LiteDB.LiteDatabase db)
            {
                this.Collection = db.GetCollection<T>();
                this.db = db;

            }

            public void Dispose()
            {
                this.db?.Dispose();

            }
        }

        public LiteDatabaseEx(string connectionString)
        {
            if (!connectionString.Contains("="))
            {
                connectionString = $"Filename={connectionString}";
            }
            this.connectionString = connectionString;
            var connection = Parse(this.connectionString);
            if (connection.TryGetValue("Filename", out var fn))
            {
                var fff = Path.GetFullPath(fn);
                if (!Directory.Exists(Path.GetDirectoryName(fff)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fff));
                }
            };
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
        protected async Task<LiteDB.LiteDatabase> Lock(bool write, CancellationToken cancellationToken)
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
        public async Task<DisposableCollection<T>> GetCollection<T>(bool readOnly, CancellationToken cancellationToken)
        {
            return new DisposableCollection<T>(await this.Lock(!readOnly, cancellationToken));
        }

    }
}
