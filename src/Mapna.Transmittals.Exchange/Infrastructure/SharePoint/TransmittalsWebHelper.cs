using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GN.Library.SharePoint;
using System.Linq;

namespace Mapna.Transmittals.Exchange.Infrastructure.SharePoint
{
    public class TransmittalsWebHelper
    {
        private readonly ClientContext context;
        private readonly ILogger logger;

        internal TransmittalsWebHelper(ClientContext context, ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        private async Task<List> EnsureList(string title, string description)
        {
            var result = context.Web.WithCollectionEx(x => x.Lists, h => h.Title)
                  .ToArray()
                  .FirstOrDefault(x => string.Compare(x.Title, title, true) == 0);
            if (result == null)
            {
                ListCreationInformation creationInfo = new ListCreationInformation();
                creationInfo.Title = title;
                creationInfo.Description = description;
                creationInfo.TemplateType = (int)ListTemplateType.GenericList;
                List newList = context.Web.Lists.Add(creationInfo);
                context.Load(newList);
                context.ExecuteQuery();
                result = context.Web.WithCollectionEx(x => x.Lists, h => h.Title)
                  .ToArray()
                  .FirstOrDefault(x => string.Compare(x.Title, title, true) == 0);
            }
            return result;

        }
        public async Task<bool> EnsureField(List list, string name, string schema)
        {
            var fields = list.WithCollectionEx(x => x.Fields, y => y.InternalName, y => y.SchemaXml).ToArray();
            var field = fields.FirstOrDefault(x => x.InternalName == name);
            if (field == null)
            {
                field = list.Fields.AddFieldAsXml(schema, true, AddFieldOptions.DefaultValue);
                field.Update();
                context.ExecuteQuery();
            }
            fields = list.WithCollectionEx(x => x.Fields, y => y.InternalName, y => y.SchemaXml).ToArray();
            field = fields.FirstOrDefault(x => x.InternalName == name);
            if (field == null)
            {
                throw new Exception(
                    $"Failed to create field '{name}'");

            }
            return field != null;
        }
        public async Task<bool> EnsureJobList()
        {
            var result = false;
            try
            {
                var log_title = "Job1";
                var lst = await EnsureList(log_title, "Transmittal Exchange Jobs");
                if (lst == null)
                {
                    throw new Exception($"Failed to EnsureList:{log_title}");
                }
                var fields = new List<Tuple<string, string>>() {
                    new Tuple<string, string>("JsonContent",
                        "<Field Type='Note' DisplayName='JsonContent' Required='FALSE'  NumLines='6' RichText='FALSE' RichTextMode='Compatible'  Sortable='FALSE'  AppendOnly='FALSE' Version='1' />"),
                    new Tuple<string, string>("SourceId",
                        "<Field Type='Text'  DisplayName='SourceId' Required='FALSE' />"),
                    new Tuple<string, string>("Status",
                        "<Field Type='Choice' DisplayName='Status' Required='FALSE'  Format='Dropdown' FillInChoice='FALSE'><Default>Info</Default><CHOICES><CHOICE>In Progress</CHOICE><CHOICE>Completed</CHOICE><CHOICE>Failed</CHOICE><CHOICE>Canceled</CHOICE><CHOICE>Postponed</CHOICE><CHOICE>Waiting</CHOICE></CHOICES></Field>"),
                    new Tuple<string, string>("State",
                        "<Field Type='Note' DisplayName='State' Required='FALSE'  NumLines='6' RichText='FALSE' RichTextMode='Compatible'  Sortable='FALSE'  AppendOnly='FALSE' Version='1' />"),
                     new Tuple<string, string>("InternalId",
                        "<Field Type='Text'  DisplayName='InternalId' Required='FALSE' />"),
                    new Tuple<string, string>("Direction",
                        "<Field Type='Choice' DisplayName='Direction' Required='FALSE'  Format='Dropdown' FillInChoice='FALSE'><Default>Info</Default><CHOICES><CHOICE>In</CHOICE><CHOICE>Out</CHOICE></CHOICES></Field>"),
                    new Tuple<string, string>("StateReason",
                        "<Field Type='Note' DisplayName='StateReason' Required='FALSE'  NumLines='6' RichText='FALSE' RichTextMode='Compatible'  Sortable='FALSE'  AppendOnly='FALSE' Version='1' />"),

                };
                foreach (var field in fields)
                {
                    if (!await EnsureField(lst, field.Item1, field.Item2))
                    {
                        throw new Exception(
                            $"Failed to ensure field '{field.Item1}' on list '{log_title}'");
                    }
                }
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to EnsureLogList. Err:{err.Message}");
            }
            return result;
        }
        public async Task<bool> EnsureLogList()
        {
            var result = false;
            try
            {
                var log_title = "Log1";
                var lst = await EnsureList(log_title, "Transmittal Exchange Logs");
                if (lst == null)
                {
                    throw new Exception($"Failed to EnsureList:{log_title}");
                }
                var fields = new List<Tuple<string, string>>() {
                    new Tuple<string, string>("Level",
                        "<Field Type='Choice' DisplayName='Level' Required='FALSE'  Format='Dropdown' FillInChoice='FALSE'  StaticName='Level' Name='Level' ><Default>Info</Default><CHOICES><CHOICE>Info</CHOICE><CHOICE>Warn</CHOICE><CHOICE>Error</CHOICE><CHOICE>Critical</CHOICE></CHOICES></Field>"),
                    new Tuple<string, string>("Message",
                        "<Field Type='Note' DisplayName='Message' Required='FALSE'  NumLines='6' RichText='FALSE' RichTextMode='Compatible'  Sortable='FALSE'  StaticName='Message' Name='Message' AppendOnly='FALSE' Version='1' />"),
                    //new Tuple<string, string>("Test",
                    //    "<Field Type='Text'  DisplayName='Test' Required='FALSE' />")


                };
                foreach (var field in fields)
                {
                    if (!await EnsureField(lst, field.Item1, field.Item2))
                    {
                        throw new Exception(
                            $"Failed to ensure field '{field.Item1}' on list '{log_title}'");
                    }
                }

            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to EnsureLogList. Err:{err.Message}");
            }
            return result;
        }
        public static async Task<bool> EnsureLists(ClientContext context, IServiceProvider serviceProvider)
        {
            var helper = new TransmittalsWebHelper(context, serviceProvider.GetService<ILogger<TransmittalsWebHelper>>());
            //await helper.EnsureJobList();

            return await helper.EnsureLogList() && await helper.EnsureJobList();
        }
    }
}
