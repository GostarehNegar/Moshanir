using GN.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library
{
    
    public interface IAppDataServices
    {
        IAppContext AppContext { get; }
        ///// <summary>
        ///// Gets LocalDataContext where application local data are stored.
        ///// This is a disposable object and should be used with 'Using' pattern
        ///// whenevere possible. e.g using (var db = AppContext.Local()) {...}
        ///// </summary>
        ///// <returns></returns>
        //ILocalDataContext Local();
        ///// <summary>
        ///// Gets a DataContext where User private data are stored.
        ///// User private data are shared among applications.
        ///// </summary>
        ///// <returns></returns>
        //IUserDataContext User();
        ///// <summary>
        ///// Gets a DataContext where Public private data are stored.
        ///// Public data are shared among applications and users.
        ///// </summary>
        ///// <returns></returns>

        //IPublicDataContext Public();
        //IGlobalDataContext Global();
    }
    class AppDataContext : IAppDataServices
    {
        public IAppContext AppContext { get; private set; }
        public AppDataContext(IAppContext ctx)
        {
            this.AppContext = ctx;
        }
    }
}
