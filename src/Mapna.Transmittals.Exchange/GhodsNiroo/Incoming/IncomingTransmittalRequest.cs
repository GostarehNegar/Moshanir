using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.GhodsNiroo.Incoming
{
    public class IncomingTransmittalRequest
    {
        public class FileModel
        {
            public string DocNumber { get; set; }
            public string Url { get; set; }
            public string Ext_Rev { get; set; }
            public string Int_Rev { get; set; }
            public string Status { get; set; }
            public string Purpose { get; set;}
            public string FileName { get; set; }
            public void Validate()
            {
                if (string.IsNullOrWhiteSpace(DocNumber))
                    throw new Exception($"'DocNumber' is null or empty");
                if (string.IsNullOrWhiteSpace(FileName))
                    throw new Exception($"'FileName' is null or empty");
                if (string.IsNullOrWhiteSpace(Url))
                    throw new Exception($"'Url' is null or empty");



            }
        }
        public string Url { get; set; }
        public string Tr_file_Name { get; set; }
        public string TR_NO { get; set; }
        public string Project_code { get; set; }
        public string Project_Name { get; set; }
        public List<FileModel> Files { get; set; }

        public override string ToString()
        {
            return $"Transmmittal {TR_NO}";
            
        }
        public IncomingTransmittalRequest Validate()
        {
            if (string.IsNullOrWhiteSpace(TR_NO))
                throw new Exception($"'TR_NO' is null or empty");
            if (string.IsNullOrWhiteSpace(Tr_file_Name))
                throw new Exception($"'Tr_file_Name' is null or empty");
            if (string.IsNullOrWhiteSpace(Url))
                throw new Exception($"'Url' is null or empty");
            if (string.IsNullOrWhiteSpace(Project_code))
                throw new Exception($"'Project_code' is null or empty");
            if (string.IsNullOrWhiteSpace(Project_Name))
                throw new Exception($"'Project_Name' is null or empty");
            Files = Files ?? new List<FileModel>();
            Files.ForEach(x => x.Validate());
            return this;



        }

    }
}
