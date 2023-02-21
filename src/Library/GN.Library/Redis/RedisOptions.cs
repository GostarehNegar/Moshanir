using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Redis
{
    public class RedisOptions
    {
        private static string DefaultConnectionString = "localhost";
        /// <summary>
        /// https://stackexchange.github.io/StackExchange.Redis/Configuration.html
        /// </summary>
        public string ConnectionString { get; set; }
        internal string GetMultiplexreConnectionString()
        {
            Validate();
            return IsConnectionStringValid(this.ConnectionString) ? this.ConnectionString : DefaultConnectionString;
        }
        public RedisOptions()
        {
            this.ConnectionString = DefaultConnectionString;
        }
        private bool IsConnectionStringValid(string connectionString)
        {
            return !string.IsNullOrWhiteSpace(connectionString);
        }
        public RedisOptions Validate()
        {
            this.ConnectionString = IsConnectionStringValid(this.ConnectionString) ? this.ConnectionString : DefaultConnectionString;
            return this;
        }
    }
}
