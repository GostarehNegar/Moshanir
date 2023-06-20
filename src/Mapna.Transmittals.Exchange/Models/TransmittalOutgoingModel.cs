using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;

namespace Mapna.Transmittals.Exchange.Models
{

    public class MapnaTransmittalFeedbackModel
    {
        public string TransmittalNumber { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDesc { get; set; }

        public int GetResponseCode()
        {
            return int.TryParse(ResponseCode, out var res) ? res : -1;
        }

    }
    public class MapnaResponseModel
    {
        public string ResponseCode { get; set; }
        public string ResponseDesc { get; set; }
    }

    public class TransmittalSubmitToMapnaModel
    {
        public class DocumentModel
        {
            public string DocNumber { get; set; }
            public string Url { get; set; }
            public string Ext_Rev { get; set; }
            public string Source_Id { get; set; }
            public string Int_Rev { get; set; }
            public string Status { get; set; }
            public string Purpose { get; set; }
            public string FileName { get; set; }
        }

        public string Url { get; set; }
        public string ReferedTo { get; set; }
        public string Source_Id { get; set; }
        public string Tr_No { get; set; }
        public string Domain { get; set; }
        public string Action { get; set; }
        public DocumentModel[] Documents { get; set; }

    }

    public class TransmittalOutgoingFileModel
    {
        public string ServerRelativePath { get; set; }
        public string DocumentNumber { get; set; }
        public string Purpose { get; set; }
        public string Staus { get; set; }
        public string ExtRev { get; set; }
        public string IntRev { get; set; }

        public string Url
        {
            get
            {
                return MapnaTransmittalsExtensions.ToUrl(this.ServerRelativePath);
            }
        }
        public string FileName
        {
            get
            {
                try
                {
                    return Path.GetFileName(this.ServerRelativePath);
                }
                catch
                {

                }
                return string.Empty;
            }
        }

        public TransmittalSubmitToMapnaModel.DocumentModel ToDocumentModel()
        {
            return new TransmittalSubmitToMapnaModel.DocumentModel
            {
                DocNumber = this.DocumentNumber,
                Purpose = this.Purpose ?? "X",
                Status = this.Staus,
                Ext_Rev = this.ExtRev,
                Int_Rev = this.IntRev,
                FileName = this.FileName,
                Url = this.Url,
                Source_Id = "MySourceId"
            };
        }

    }
    public class TransmittalOutgoingModel
    {
        public string TransmitallNumber { get; set; }
        public string TransmittalTitle { get; set; }
        public TransmittalOutgoingFileModel[] Files { get; set; }

        public string Url { get; set; }
        public string LetterFileName { get; set; }

        public string ToXml()
        {
            var xml = new XmlDocument();
            //XmlDeclaration xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            //XmlElement root = xml.DocumentElement;
            //xml.InsertBefore(xmlDeclaration, root);
            //xml.LoadXml("<transmittal ></transmittal>");
            var trans = xml.CreateElement("transmittal");
            trans.SetAttribute("url", this.Url);
            trans.SetAttribute("attach_filename", this.LetterFileName);
            trans.SetAttribute("internal_letter_no", this.TransmitallNumber);
            xml.AppendChild(trans);

            foreach (var file in this.Files ?? Array.Empty<TransmittalOutgoingFileModel>())
            {
                var doc = xml.CreateElement("document");
                doc.SetAttribute("url", file.Url);
                doc.SetAttribute("doc_no", file.DocumentNumber);
                doc.SetAttribute("client_file_name", file.FileName);
                doc.SetAttribute("status", file.Staus);
                doc.SetAttribute("purpose", file.Purpose);
                doc.SetAttribute("ext_rev", file.ExtRev);

                trans.AppendChild(doc);
            }



            return xml.OuterXml;
        }

        public TransmittalSubmitToMapnaModel ToSubmitModel()
        {
            var result = new TransmittalSubmitToMapnaModel
            {
                Url = this.Url,
                Tr_No = this.TransmitallNumber,
                Action = "0",
                Domain = "Moshanir",
                ReferedTo = "AS-MD2-FAB-T-0002",
                Source_Id = "My Source Id"
            };
            result.Documents = this.Files.Select(x => x.ToDocumentModel()).ToArray();
            return result;

        }
    }
}
