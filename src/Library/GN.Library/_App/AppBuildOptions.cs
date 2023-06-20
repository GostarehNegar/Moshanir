using GN.Library.Collections;
using GN.Library.Configurations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library
{
    public enum HostTypes
    {
        Generic,
        Web
    }
    public class AppBuildOptions //: AppSettings<AppBuildOptions>
    {
        public static AppBuildOptions Current = new AppBuildOptions();
        private readonly ObjectCollection_Deprecated objectCollection = new ObjectCollection_Deprecated();

        public ObjectCollection_Deprecated Properties => this.objectCollection;
        public HostTypes HostType { get; private set; }
        public AppBuildOptions()
        {
            this.MimimumLogLevel = LogLevel.Information;
            this.HostType = HostTypes.Web;
        }
        public bool AddMessageBus
        {
            get { return this.Properties.GetOrAdd<bool>("$AddMessageBus", true); }
            set { this.Properties.Update<bool>("$AddMessageBus", value); }
        }
        public bool AddXrmMessageBus
        {
            get { return this.Properties.GetOrAdd<bool>("$AddXrnMessageBus", true); }
            set { this.Properties.Update<bool>("$AddXrmMessageBus", value); }
        }
        public LogLevel MimimumLogLevel { get; set; }
        public AppBuildOptions UseMessageBus()
        {
            this.AddMessageBus = true;
            return this;
        }
        public AppBuildOptions UseXrmMessageBus()
        {
            this.AddXrmMessageBus = true;
            return this;
        }
        public AppBuildOptions UseGenericHost()
        {
            this.HostType = HostTypes.Generic;
            return this;
        }
        public string NLogFileName
        {
            get { return this.Properties.GetOrAdd<string>("$NLogConfig", ""); }
            set { this.Properties.Update<string>("$NLogConfig", value); }

        }
        public AppBuildOptions UseNLog(string configFileName)
        {
            NLogFileName = configFileName;
            return this;
        }
        public AppInfo AppInfo
        {
            get { return this.Properties.GetOrAdd<AppInfo>("$AppInfo", AppInfo.Current); }
        }
    }
    
}
