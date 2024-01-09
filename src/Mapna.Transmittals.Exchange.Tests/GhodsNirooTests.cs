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
using Microsoft.SharePoint.Client;
using Mapna.Transmittals.Exchange.GhodsNiroo;

namespace Mapna.Transmittals.Exchange.Tests
{

    [TestClass]
    public class GhodsNirooTests
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
                   //s.AddTransmittalsExchange(c.Configuration, cfg =>
                   //{
                   //    // cfg.ConnectionString = "Url=http://dcc.moshanir.co:90/nogonbad;UserName=tem_dc;Password=D@c2023;Domain=moshanir";
                   //});
                   s.AddGhodsNiroo(c.Configuration, opt => { });
               })
               .Build()
               .UseGNLib();
        }
        [TestMethod]
        public async Task Test1()
        {
            var host = this.GetHost();
            //await host.StartAsync();
            var ctx = new IncomingTransmittalContext(host.Services, new GhodsNirooTransmittalOptions().Validate(), new GhodsNiroo.Incoming.IncomingTransmittalRequest());
            //await ctx.GetSPContext().SendLogAsync(Microsoft.Extensions.Logging.LogLevel.Information, "TEST", "test message");
            var content = System.IO.File.ReadAllBytes("pdf1.zip");
            await ctx.GetSPContext().UploadTransmittal("J4-MED-GNC-T-1366.zip", content, "J4-MED-GNC-T-1366", "TEST", opt => { });





        }
        [TestMethod]
        public async Task Test2()
        {
            var host = this.GetHost();
            await host.StartAsync();
            var ctx = new IncomingTransmittalContext(host.Services, new GhodsNirooTransmittalOptions().Validate(), new GhodsNiroo.Incoming.IncomingTransmittalRequest());
            host.Services.GetService<IGhodsNirooIncomingQueue>().Enqueue(new GhodsNiroo.Incoming.IncomingTransmittalRequest { 

                TR_NO = "J4-MED-GNC-T-1369",
                Project_Name ="Test",
                Url= "https://raw.githubusercontent.com/GostarehNegar/Moshanir/main/README.md",
                Tr_file_Name ="TEST.txt",
                Files= new List<GhodsNiroo.Incoming.IncomingTransmittalRequest.FileModel>
                {
                    new GhodsNiroo.Incoming.IncomingTransmittalRequest.FileModel
                    {
                        Url = "https://raw.githubusercontent.com/GostarehNegar/Moshanir/main/README.md",
                        FileName = "test.zip",
                        DocNumber = "TEST-1",
                        Purpose = "FA",
                        Int_Rev = "B",
                        Ext_Rev = "0",
                        Status = "C4"
                    }
                }
            });

            await Task.Delay(200000);





        }

    }
}
