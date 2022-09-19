using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Diagnostics;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        var dbConnection = Environment.GetEnvironmentVariable("LogConnection", EnvironmentVariableTarget.Process);
        var sinkOpts = new MSSqlServerSinkOptions();
        sinkOpts.TableName = "Logs";
        sinkOpts.AutoCreateSqlTable = true;
        var columnOpts = new ColumnOptions();
        columnOpts.Store.Remove(StandardColumn.Properties);
        columnOpts.Store.Add(StandardColumn.LogEvent);
        columnOpts.LogEvent.DataLength = 2048;
        columnOpts.PrimaryKey = columnOpts.TimeStamp;
        columnOpts.TimeStamp.NonClusteredIndex = true;
        var logger = new LoggerConfiguration()
        .Enrich.WithProperty("ApplicationContext", "FunctionApp1")
        .WriteTo.Console()
        .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.MSSqlServer(
            connectionString: dbConnection,
            sinkOptions: sinkOpts,
            columnOptions: columnOpts)
        .CreateLogger();
        s.AddLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddSerilog(logger);
            });
        Serilog.Debugging.SelfLog.Enable(msg =>
        {
            Debug.Print(msg);
            Debugger.Break();
        });
    })
    .Build();

host.Run();
