using GN.Library.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GN.Library
{
    public class LibOptions
    {
        public class UserServicesOptions
        {
            public bool Enabled { get; set; }
        }
        public class HealthCheckOptions
        {
            public bool Enabled { get; set; }
            public int  FrequencyInMinutes { get; set; }
        }
        public HealthCheckOptions HealthCheck { get; set; }
        public UserServicesOptions UserService { get; set; }
        public static LibOptions Current = new LibOptions();
        public static LibOptions Default => Current;
        public LibOptions()
        {
            this.HealthCheck = new HealthCheckOptions();
            this.UserService = new UserServicesOptions();
        }
        public LibOptions Validate()
        {
            this.HealthCheck = this.HealthCheck?? new HealthCheckOptions();
            this.UserService = this.UserService?? new UserServicesOptions();
            return this;

        }
        public string DocumentStoreDefaultDirectory => "./Data";
        public string DocumentStoreDefaultFileName => "db.dat";
        public string GetLocalDbFileName()
        {
            var result = Path.GetFullPath($"./Data/{AppHost.AppInfo.Name}.db");
            if (!Directory.Exists(Path.GetDirectoryName(result)))
                Directory.CreateDirectory(Path.GetDirectoryName(result));
            return result;

        }
        public string GetCommonApplicationDataFolder()
        {
            var result = Path.GetDirectoryName(
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Gostareh Negar\\sample.dat"));
            if (!Directory.Exists(result))
                Directory.CreateDirectory(result);
            return result;
        }
        public string GetUserDbFileName()
        {
            var result = Path.GetFullPath(
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Gostareh Negar\\data\\user.db"));
            if (!Directory.Exists(Path.GetDirectoryName(result)))
                Directory.CreateDirectory(Path.GetDirectoryName(result));
            return result;
        }
        public string GetPublicDbFileName()
        {
            var result = Path.GetFullPath(
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Gostareh Negar\\Data\\public.db"));
            if (!Directory.Exists(Path.GetDirectoryName(result)))
                Directory.CreateDirectory(Path.GetDirectoryName(result));
            return result;
        }
        public string GetGlobalDbFileName()
        {
            var result = Path.GetFullPath(
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                //@"\\BABAK-PC\Data\global.db"));
                "Gostareh Negar\\Data\\global.db"));
            result = @"\\BABAK-PC\Data\global.db";

            //\\BABAK-PC\Data
            if (!Directory.Exists(Path.GetDirectoryName(result)))
                Directory.CreateDirectory(Path.GetDirectoryName(result));
            return result;
        }
        internal Action<Redis.RedisOptions> redis_config;
        internal bool skip_redis;
        public void AddRedis(Action<Redis.RedisOptions> cfg = null)
        {

            this.redis_config = cfg;
        }
        public void SkipRedis()
        {
            this.skip_redis = true;

        }

        public bool Disabled { get; set; }
        public AppInfo AppInfo { get { return AppInfo.Current; } }
    }
}
