using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using GN.Library.Data;
using GN.Library.Data.Deprecated;

namespace GN.Library.Helpers
{
    public interface IAppServerExplorer
    {
        IEnumerable<AppInfo> GetAll(bool refersh = false);
        void Update();
        string GetMessageServerUrl();
    }
    class AppServerExplorer : IAppServerExplorer
    {
        private List<AppInfo> appInfos;
        private string fileName;
        private IAppUtils utils;

        
        public AppServerExplorer()
        {
            fileName = Path.Combine(
                LibOptions.Default.GetCommonApplicationDataFolder(),
                "AppServers.dat");
        }

        //public IEnumerable<AppInfo> _GetAll(bool refersh = false)
        //{

        //}

        public IEnumerable<AppInfo> GetAll(bool refersh = false)
        {
            if (this.appInfos == null || refersh)
            {
                this.appInfos = new List<AppInfo>();
                var myinfo = AppInfo.Current;
                myinfo.Validate();

                if (File.Exists(this.fileName))
                {
                    try
                    {
                        var data = AppHost.Utils.Deserialize<List<AppInfo>>(File.ReadAllText(this.fileName));
                        if (data != null)
                            this.appInfos.AddRange(data);
                    }
                    catch { }
                }
                var me = this.appInfos.FirstOrDefault(x => x.Name == myinfo.Name);
                if (me != null)
                {
                    this.appInfos.Remove(me);
                }
                this.appInfos.Add(myinfo);
            }
            return this.appInfos;
        }
        public void Update()
        {
            var myinfo = AppInfo.Current;
            myinfo.Validate();
            var all = GetAll().ToList();
            if (!all.Any(x => x.Name == myinfo.Name))
            {
                all.Add(myinfo);
            }
            try
            {
                File.WriteAllText(this.fileName, AppHost.Utils.Serialize(all));
            }
            catch
            {
            }

        }
        public string GetMessageServerUrl()
        {
            string result = null;
            var myinfo = AppInfo.Current;
            myinfo.Validate();
            var apps = this.GetAll(true)
                .Where(x => x.Name != myinfo.Name)
                .Where(x => x.IsMessageServer).FirstOrDefault();
            if (apps != null)
            {
                result = apps.Urls.Split(',')[0];
            }
            return result;
        }
    }
}
