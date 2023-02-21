using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals
{
    public class SPFolder
    {
        private readonly Folder folder;

        public Folder Folder => folder;
        public ClientContext Context => this.folder.Context.Extend();
        public SPFolder(Folder folder)
        {
            this.folder = folder;
        }
        public async Task<SPFolder[]> GetFolders()
        {
            foreach(var folder in this.folder.Folders)
            {
                this.Context.Load(folder, f => f.Folders);
                
            }
            await this.Context.ExecuteQueryAsync();
            return this.folder.Folders.Select(x => new SPFolder(x)).ToArray();
        }
        public string Name => this.Folder?.Name;


    }
}
