using Microsoft.VisualStudio.TestTools.UnitTesting;
using GN.Library;
using GN.Library.Messaging;
using GN;
using GN.Library.SharePoint;
using Microsoft.Extensions.Hosting;
using Mapna.Transmittals.Exchange.Services;
using Mapna.Transmittals.Exchange.Internals;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System;
using System.Net.Http;
using System.IO;
using System.Threading;
using System.Net;
using GN.Library.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Collections.Generic;
using System.Collections;
using GN.Library.SharePoint.Internals;
using Microsoft.Extensions.Configuration;
using Mapna.Transmittals.Exchange.Domain.Outgoing;
using Mapna.Transmittals.Exchange.Models;

namespace Mapna.Transmittals.Exchange.Tests
{
    [TestClass]
    public class UnitTests
    {
        private IHost GetHost()
        {
            return AppHost.GetHostBuilder()
               .ConfigureAppConfiguration(cfg => cfg.AddJsonFile("appsettings.json"))
               .ConfigureServices((c, s) =>
               {
                   s.AddGNLib(c.Configuration, cfg => { });
                   s.AddMessagingServices(c.Configuration, cfg => { });
                   s.AddSharePointServices(c.Configuration, cfg => { });
                   s.AddTransmittalsExchange(c.Configuration, cfg =>
                   {
                       cfg.ConnectionString = "Url=http://dcc.moshanir.co:90/ardakan;UserName=tem_dc;Password=D@c2023;Domain=moshanir";
                   });
               })
               .Build()
               .UseGNLib();
        }

        [TestMethod]
        public async Task Connection_IsOk()
        {
            var list = await ClientContextFactory
                .CreateContext("Url=http://dcc.moshanir.co:90/ardakan;UserName=tem_dc;Password=D@c2023;Domain=moshanir", null)
                .Web
                .GetListByPath("/Log/");
            await list.InsertItem<SPLogItem>(new SPLogItem
            {
                Message = "hello world",
                Level = SPLogItem.Schema.Levels.Info,
                Title = "Babak"
            });
            var items = await list.GetQueryable<SPLogItem>()
                .Where(x=>x.Title=="babak")
                .Take(10).ToArrayAsync();


        }

        [TestMethod]
        public async Task transmittal_list_wroks()
        {
            var host = GetHost();
            var ctx = ClientContextFactory
                .CreateContext("Url=http://dcc.moshanir.co:90/ardakan;UserName=tem_dc;Password=D@c2023;Domain=moshanir", null);
            var list = await ctx.Web.Extend<SPTransmittalWeb>().GetTransmitalsList();
            var items = await list
                .GetQueryable(cfg => cfg.WithColums(SPTransmittalItem.Schema.DefaultFields))
                .Take(10)
                .ToArrayAsync();
            var trans = new SPTransmittalItem
            {
                ToLook = new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = 56 },
                Title = "test2",
                TrAction = "FirstIssue",
                LetterNo = "TR-01",
                DiscFirstLook0 = new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = 39 },
                TrDateHijri = DateTime.Now.AddDays(0)


            };
            try
            {
                await list.InsertItem(trans);
            }
            catch (Exception err)
            {

            }
            var item = await list.GetQueryable(cfg => cfg.WithColums(SPTransmittalItem.Schema.DefaultFields))
                .Where(x => x.LetterNo == "TR-01")
                .FirstOrDefaultAsync();

        }

        [TestMethod]
        public async Task can_find_documents_in_masterlist()
        {
            var host = GetHost();

            var list = host.Services.GetService<ITransmittalRepository>();
            var item = await list.FindInMasterList("MD2-AS-00-BP-I-10-PI0-101");

        }



        [TestMethod]
        public async Task transmittal_repository_wroks()
        {
            var host = GetHost();
            //var ctx = ClientContextFactory
            //    .CreateContext("Url=http://dcc.moshanir.co:90/ardakan;UserName=tem_dc;Password=D@c2023;Domain=moshanir", null);
            ITransmittalRepository service = host.Services.GetServiceEx<ITransmittalRepository>();


            var trans = await service.GetOrAddTransmittal("MAPNA-TRANS-17", cfg =>
            {
                cfg.ToLook = new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = 56 };
                cfg.Title = "test4";
                cfg.TrAction = "FirstIssue";
                //cfg.LetterNo = "TR-01";
                cfg.DiscFirstLook0 = new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = 39 };
                //TrDateHijri = DateTime.Now.AddDays(15)
            });

        }
        [TestMethod]
        public async Task DocLib_works1()
        {
            var host = GetHost();
            var service = host.Services.GetServiceEx<ITransmittalRepository>();



        }
        [TestMethod]
        public async Task transmittal_repository_jobs()
        {
            var host = GetHost();
            var service = host.Services.GetServiceEx<ITransmittalRepository>();
            var sourceId = Guid.NewGuid().ToString();
            await service.CreateJob(new SPJobItem { SourceId = sourceId, Title = "Test", Content = "{}" });
            var job = await service.FindJob(sourceId);
            Assert.IsNotNull(job);
            Assert.AreEqual(sourceId, job.SourceId);
            job.Title = "Receive";
            await service.UpdateJob(job);
            var in_progess = await service.GetInProgressJobs();
            Assert.IsTrue(in_progess.Any(x => x.SourceId == sourceId));

            job.SetCompleted();
            await service.UpdateJob(job);
            in_progess = await service.GetInProgressJobs();
            Assert.IsFalse(in_progess.Any(x => x.SourceId == sourceId));
            await service.DeleteJob(job);
            in_progess = await service.GetInProgressJobs();
            Assert.IsFalse(in_progess.Any(x => x.SourceId == sourceId));


        }
        [TestMethod]
        public async Task transmittal_service_wroks()
        {
            var host = GetHost();
            var service = host.Services.GetServiceEx<ITransmittalService>();
            await host.StartAsync();
            TransmittalFileSubmitModel[] files = new TransmittalFileSubmitModel[]
            {
                new TransmittalFileSubmitModel
                {
                    DocNumber = "MD2-AS-00-BP-I-10-PI0-101",
                    FileName = "MD2-AS-00-BP-I-10-PI0-101_A_A.pdf",
                    Url =  "https://raw.githubusercontent.com/GostarehNegar/Moshanir/main/File1.pdf",
                    Purpose="K",
                    Int_Rev="A",
                    Ext_Rev="B",
                    Status="S",
                },
                new TransmittalFileSubmitModel
                {
                    DocNumber = "MD2-AS-00-BP-I-11-IO3-101",
                    FileName = "MD2-AS-00-BP-I-11-IO3-101_B_B.pdf",
                    Url =  "https://raw.githubusercontent.com/GostarehNegar/Moshanir/main/1.zip",
                    Purpose="K",
                    Int_Rev="A",
                    Ext_Rev="B",
                    Status="S",
                }
            };
            var result = await service.Submit(new TransmittalSubmitModel
            {
                TR_NO = "MAPNA-TRANS-13",
                Title = "SAMPLE",
                //Source_Id = Guid.NewGuid().ToString(),
                Url = "https://gnco.ir",
                Documents = files

            });
            await Task.Delay(1000000);
        }
        [TestMethod]
        public async Task transmittal_validate_specs()
        {
            var host = GetHost();
            await Task.CompletedTask;
            var service = host.Services.GetServiceEx<ITransmittalService>();
            var trans = new TransmittalSubmitModel();
            Assert.IsFalse(new TransmittalSubmitModel().TryValidate(out var message));
            //trans.Source_Id = Guid.NewGuid().ToString();
            trans.TR_NO = "TRANS-02";
            Assert.IsFalse(trans.TryValidate(out message));
            trans.Url = "https://www.gnco.ir";
            Assert.IsFalse(trans.TryValidate(out message));
            var file = new TransmittalFileSubmitModel { };
            trans.Documents = new TransmittalFileSubmitModel[] { file };
            Assert.IsFalse(trans.TryValidate(out message));
            file.FileName = "DOCUMENT_1.pdf";
            Assert.IsFalse(trans.TryValidate(out message));
            file.Url = "https://gnco.ir";
            Assert.IsFalse(trans.TryValidate(out message));
            file.Status = "STAT";
            Assert.IsFalse(trans.TryValidate(out message));
            file.Int_Rev = "A";
            file.Ext_Rev = "B";
            Assert.IsFalse(trans.TryValidate(out message));
            file.Purpose = "PURP";
            Assert.IsTrue(trans.TryValidate(out message));

        }



        [TestMethod]
        public async Task download_queue_works()
        {
            var host = this.GetHost();
            await host.StartAsync();
            var target = host.Services.GetServiceEx<IFileDownloadQueue>();
            //https://mycart.mapnagroup.com/group_app/ws_dc/moshanirgetfile/tr/1428419
            var url = "https://mycart.mapnagroup.com/group_app/ws_dc/moshanirgetfile/tr/1428419";
            var ctx = target.Enqueue(url, "1.zip");
            //var ctx = target.Enqueue("https://raw.githubusercontent.com/GostarehNegar/Moshanir/main/1.zip", "1.zip");
            //https://raw.githubusercontent.com/GostarehNegar/Moshanir/main/ngrok-v3-stable-windows-amd64.zip
            //var ctx1 = target.Enqueue("https://raw.githubusercontent.com/GostarehNegar/Moshanir/main/ngrok-v3-stable-windows-amd64.zip", "1.zip");

            ctx.OnCompleted += (a, b) =>
            {
            };
            ctx.OnProgress += (a, b) =>
            {
                if (b.ProgressPercentage > 3)
                {

                }
            };

            _ = Task.Run(async () =>
            {
                await Task.Delay(100000);
                ctx.Cancel();
            });
            /// We can await for completion task.
            /// 
            await ctx.CompletionTask;


            await Task.Delay(6000);
            ctx.Cancel();
        }

        [TestMethod]
        public async Task DocLib_works()
        {
            var cccc = "/llkklk/test.jpg";

            var host = this.GetHost();
            var target = host.Services.GetServiceEx<ITransmittalRepository>();
            var _f = Path.GetDirectoryName("/mmm/nn.jpf");
            var __f = Path.GetFileName("/mmm/nn.jpf");
            //var options = host.Services.GetServiceEx<TransmittalsExchangeOptions>();
            //var context = host.Services.GetServiceEx<IClientContextFactory>()
            //    .CreateContext(SPConnectionString.Parse(options.ConnectionString));
            //await target.Test("");

            var items2 = await target.GetPendingJobs();
            var stream = new FileStream("c:\\temp\\1.jpg", FileMode.Open);
            //var trans = await target.GetTransmittal("MD2-MOS-4");
            var f = await target.UploadDoc("/MD2-MOS-3", "MD2-AS-00-EL-E-02-EG0-001_B_B.jpg", stream);
            var file = await target.GetDocumentByPath(f);


        }

        [TestMethod]
        public async Task TEMP()
        {
            var host = this.GetHost();
            var target = host.Services.GetServiceEx<ITransmittalRepository>();

            var options = host.Services.GetServiceEx<TransmittalsExchangeOptions>();
            var context = host.Services.GetServiceEx<IClientContextFactory>()
                .CreateContext(options.ConnectionString);

            //await context.Web.With(x => x.Lists)
            //    .DoAsync(async w =>
            //    {
            //        var list = await w.GetListByPath("/TrList");
                    

            //    });

            await target.Test("");


        }

        [TestMethod]
        public async Task Temp1()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("url", "kkk");
            client.DefaultRequestHeaders.TryAddWithoutValidation("TR_NO", "NO");


            var body = "<transmittal internal_letter_no=\"TRANS_01\" sourceid=\"id1\" referred_to=\"\" attach_filename=\"test.pdf\" url=\"http://gnco.ir\" >"+
                "<document>" +
                "</document>"+
                "</transmittal> ";
            var response = await client.PostAsync("https://mycart.mapnagroup.com/group_app/ws_dc/npx/nepaco/",
                new StringContent(System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(body))));
            var res =await  response.Content.ReadAsStringAsync();            

        }
        [TestMethod]
        public async Task SendTransmittal()
        {
            var host = this.GetHost();
            var repo = host.Services.GetService<ITransmittalRepository>();

            var b = await repo.GetWaitingTransmittals();

            return;
            var docs = await repo.GetDocumentsByTransmittal("MD2-MOS-46");
            //var trans1 = await repo.GetTransmittal("MD2-MOS-59");
            //trans1.SendFormal = "Yes";
            //trans1.IssueState = SPTransmittalItem.Schema.IssueStates.Accept.ToString();
            await repo.SetTransmittalIssueState("MD2-MOS-59", SPTransmittalItem.Schema.IssueStates.Preparing);
            




            var trans = new TransmittalOutgoingModel
            {
                TransmitallNumber = "some number"
            };
            var f = trans.ToXml();

            await host.StartAsync();

            var queue = host.Services.GetService<IOutgoingQueue>();
            queue.Enqueue("MD2-MOS-46");

            await Task.Delay(60 * 1000);


            

        }




    }

}
