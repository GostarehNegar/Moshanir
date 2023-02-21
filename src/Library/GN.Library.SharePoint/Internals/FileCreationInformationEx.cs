using Microsoft.SharePoint.Client;
using System.IO;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals
{
    public class AttachmentCreationInformationEx : AttachmentCreationInformation
    {

    }
    public class FileCreationInformationEx : FileCreationInformation
    {
        public FileCreationInformationEx WithFileName(string name)
        {
            this.Url = name;
            return this;
        }
        public FileCreationInformationEx WithContent(Stream content)
        {
            //this.ContentStream = content;
            var mem = new MemoryStream();
            content.CopyTo(mem);
            this.Content = mem.ToArray();
            return this;
        }
        public async Task<FileCreationInformationEx> WithContentAsync(Stream content)
        {
            var mem = new MemoryStream();
            await content.CopyToAsync(mem);
            this.Content = mem.ToArray();
            return this;
        }
        public async Task<FileCreationInformationEx> LoadFromFileAsync(string fileName)
        {
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            if (string.IsNullOrWhiteSpace(this.Url))
            {
                this.Url = Path.GetFileName(fileName);
            };
            await WithContentAsync(fileStream);
            return this;
        }
        public FileCreationInformationEx LoadFromFile(string fileName)
        {
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            if (string.IsNullOrWhiteSpace(this.Url))
            {
                this.Url = Path.GetFileName(fileName);
            };
            WithContent(fileStream);
            return this;
        }
        public FileCreationInformationEx WithOverride(bool @override)
        {
            this.Overwrite = @override;
            return this;
        }


    }
}
