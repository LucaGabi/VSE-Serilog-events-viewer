# VSE Serilog events viewer
Visual Studio Code Extention to inspect serilog events

This vscode extention allows inspection of logs created with serilog sinks here: https://github.com/serilog/serilog-sinks-mongodb

**Features:**
- filter by time frame, level, content
- filter by expresion (similar to where clause in SQL)
- persist config file for future inspection
- shortcuts for fast use:
    - 'f' toggle filter panel
    - 'esc' toggle expression filtering
    - inside expression editor 'ctrl-up' & 'ctrl-down' sets previous or next expression in current session history
    - 'a' move time-frame back
    - 'x' move time-frame forward    
    - 'e' filter by level error
    - 'w' filter by level warning
    - 'd' filter by level debug
    - 'i' filter by level info
    - 'q' filter by level info

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate?hosted_button_id=5MS8L5EVWBEUC)

**Create new connection**

![](https://github.com/LucaGabi/VSE-Serilog-events-viewer/blob/main/l.c.gif)

**Open from existing config**

![](https://github.com/LucaGabi/VSE-Serilog-events-viewer/blob/main/l.o.gif)

**Serilog setup**
```c#

public class JSSTimeStamp : ILogEventEnricher
{
    static long t70 = new DateTime(1970, 01, 01).ToLocalTime().Ticks;
    static long js7() => (long)TimeSpan.FromTicks(DateTime.Now.ToLocalTime().Ticks - t70).TotalMilliseconds;
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(new LogEventProperty("ts", new ScalarValue(js7())));
    }
}

....

 var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Origin", "Sample ingest")
                .Enrich.WithAssemblyName()
                .Enrich.WithCorrelationId()
                .Enrich.WithMemoryUsage()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.With<JSSTimeStamp>()
                .WriteTo.Conditional(_ => true, wt => wt.Console())
                .WriteTo.MongoDBBson(cfg =>
                {
                    // custom MongoDb configuration
                    //var mongoDbSettings = new MongoClientSettings
                    //{
                    //    UseTls = false,
                    //    AllowInsecureTls = true,
                    //    Credential = MongoCredential.CreateCredential("DBNAME", "USER", "PASSWORD"),
                    //    Server = new MongoServerAddress("127.0.0.1",27017)
                    //};

                    var mongoDbInstance = new MongoClient(
                        "mongodb://USER:PASSWORD@127.0.0.1:27017/")
                    .GetDatabase("DBNAME");

                    // sink will use the IMongoDatabase instance provided
                    cfg.SetMongoDatabase(mongoDbInstance);
                })
                .CreateLogger();
```
