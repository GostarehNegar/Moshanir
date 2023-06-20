using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GN.Library.Messaging.Queues
{
    public class MessagingQueueOptions
    {
        public bool Enabled { get; set; }
        public string Folder;

        public MessagingQueueOptions()
        {
            Folder = ".\\queues";
        }
        
        internal string GetFolderFullPath()
        {
            var folder = Path.GetFullPath(this.Folder);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }
        internal string GetQueueFullFileName(string queue)
        {
            return Path.Combine(this.GetFolderFullPath(), $"{queue}.que");

        }
        public MessagingQueueOptions Validate()
        {
            
            return this;
        }

    }
}
