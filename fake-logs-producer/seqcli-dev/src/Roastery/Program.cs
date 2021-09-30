using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Roastery.Agents;
using Roastery.Api;
using Roastery.Data;
using Roastery.Fake;
using Roastery.Util;
using Roastery.Web;
using SeqCli.Ingestion;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Roastery
{

    // Named this way to make stack traces a little more believable :-)
    public static class Program
    {
        private const string ConnectionString = "mongodb://admin:admin%40mango!!@192.168.100.2:27017/";
        private const string DB = "mongodbVSCodePlaygroundDB";

        //private const string Path = @"c:\temp\jsonl\data.json";

        public static async Task Main()
        {
            ILogger logger = default; CancellationToken cancellationToken = default;

            var buffer = new BufferingSink();

            logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Origin", "seqcli sample ingest")
                .Enrich.WithAssemblyName()
                .Enrich.WithCorrelationId()
                .Enrich.WithMemoryUsage()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Conditional(_ => true, wt => wt.Console())
                .WriteTo.Sink(buffer)
                .WriteTo.MongoDBBson(cfg =>
                {
                    // custom MongoDb configuration
                    //var mongoDbSettings = new MongoClientSettings
                    //{
                    //    UseTls = false,
                    //    AllowInsecureTls = true,
                    //    Credential = MongoCredential.CreateCredential("mongodbVSCodePlaygroundDB", "admin", "admin@mango!!"),
                    //    Server = new MongoServerAddress("127.0.0.1",27017)
                    //};

                    //var mongoDbInstance = new MongoClient(mongoDbSettings).GetDatabase("mongodbVSCodePlaygroundDB");
                    var mongoDbInstance = new MongoClient(
                        ConnectionString).GetDatabase(DB);

                    // sink will use the IMongoDatabase instance provided
                    cfg.SetMongoDatabase(mongoDbInstance);
                })
                //.WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), Path)
                .CreateLogger();

            var webApplicationLogger = logger.ForContext("Application", "Roastery Web Frontend");

            var database = new Database(webApplicationLogger, "roastery");
            DatabaseMigrator.Populate(database);

            var client = new HttpClient(
                "https://roastery.datalust.co",
                new NetworkLatencyMiddleware(
                    new RequestLoggingMiddleware(webApplicationLogger,
                        new SchedulingLatencyMiddleware(
                            new FaultInjectionMiddleware(webApplicationLogger,
                                new Router(new Controller[]
                                {
                                    new OrdersController(logger, database),
                                    new ProductsController(logger, database)
                                }, webApplicationLogger))))));

            var agents = new List<Agent>();

            for (var i = 0; i < 100; ++i)
                agents.Add(new Customer(client, Person.Generate(), (int)Distribution.Uniform(60000, 180000)));

            for (var i = 0; i < 3; ++i)
                agents.Add(new WarehouseStaff(client));

            var batchApplicationLogger = logger.ForContext("Application", "Roastery Batch Processing");
            agents.Add(new CatalogBatch(client, batchApplicationLogger));
            agents.Add(new ArchivingBatch(client, batchApplicationLogger));

            await Task.WhenAll(agents.Select(a => Agent.Run(a, cancellationToken)));

            System.Console.WriteLine("Done");
        }
    }
}
