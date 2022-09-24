using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Templates;
using System;
using System.Diagnostics;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        var dbConnection = Environment.GetEnvironmentVariable("LogConnection", EnvironmentVariableTarget.Process);
        var appInsightsInstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
        var homePath = Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process);
        var path = homePath + "\\LogFiles\\Application\\Functions\\Host\\log.txt";

        // HEAD_MONITOR_TABLE Options and Configuration
        var sinkOptsHead = new MSSqlServerSinkOptions
        {
            TableName = "HEAD_MONITOR_TABLE",
            AutoCreateSqlTable = true
        };

        var columnOptsHead = new ColumnOptions();

        // Override the default Primary Column of Serilog by custom column
        columnOptsHead.Id.ColumnName = "GLOBAL_UUID";

        // Removing all the unencessary columns
        columnOptsHead.Store.Remove(StandardColumn.Id);
        columnOptsHead.Store.Remove(StandardColumn.Message);
        columnOptsHead.Store.Remove(StandardColumn.MessageTemplate);
        columnOptsHead.Store.Remove(StandardColumn.Level);
        columnOptsHead.Store.Remove(StandardColumn.TimeStamp);
        columnOptsHead.Store.Remove(StandardColumn.Exception);
        columnOptsHead.Store.Remove(StandardColumn.Properties);

        // Adding all the custom columns
        columnOptsHead.AdditionalColumns = new List<SqlColumn>
        {
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "GLOBAL_UUID", DataLength = 50, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "USE_CASE", DataLength = 255, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "ORDER_STATUS", DataLength = 50, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.DateTime, ColumnName = "TIMESTAMP", DataLength=-1, AllowNull = false}
        };

        // DETAILS_MONITOR_TABLE Options and Configuration
        var sinkOptsDetails = new MSSqlServerSinkOptions
        {
            TableName = "DETAILS_MONITOR_TABLE",
            AutoCreateSqlTable = true
        };

        var columnOptsDetails = new ColumnOptions();

        // Removing all the unencessary columns
        columnOptsDetails.Store.Remove(StandardColumn.Id);
        columnOptsDetails.Store.Remove(StandardColumn.Message);
        columnOptsDetails.Store.Remove(StandardColumn.MessageTemplate);
        columnOptsDetails.Store.Remove(StandardColumn.Level);
        columnOptsDetails.Store.Remove(StandardColumn.TimeStamp);
        columnOptsDetails.Store.Remove(StandardColumn.Exception);
        columnOptsDetails.Store.Remove(StandardColumn.Properties);

        // Adding all the custom columns
        columnOptsDetails.AdditionalColumns = new List<SqlColumn>
        {
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "GLOBAL_UUID", DataLength = 50, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "USE_CASE", DataLength = 255, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "REQUEST_UUID", DataLength = 50, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "SERVICE", DataLength = 255, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "END_SYSTEM", DataLength = 50, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "STATE", DataLength = 50, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "PAYLOAD", DataLength=-1, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.DateTime, ColumnName = "TIMESTAMP", DataLength=-1, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "LEVEL_TYPE", DataLength = 50, AllowNull = false},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "EXCEPTION", DataLength=-1, AllowNull = true},
            new SqlColumn { DataType = System.Data.SqlDbType.NVarChar, ColumnName = "STACK_TRACE", DataLength=-1, AllowNull = true}
        };

        var logger = new LoggerConfiguration()
        .Enrich.WithProperty("ApplicationContext", "Mediator")
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Worker", LogEventLevel.Warning)
        .MinimumLevel.Override("Host", LogEventLevel.Warning)
        .MinimumLevel.Override("Host.Aggregator", LogEventLevel.Warning)
        .MinimumLevel.Override("Host.Results", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Error)
        .MinimumLevel.Override("Function", LogEventLevel.Error)
        .MinimumLevel.Override("Azure.Storage.Blobs", LogEventLevel.Error)
        .MinimumLevel.Override("Azure.Core", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.Azure.WebJobs", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.Azure.Functions.Worker", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.Azure.Functions.Worker.Http", LogEventLevel.Error)
        .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
        .WriteTo.Logger(l =>
        {
            l.WriteTo.Conditional(ev =>
            {
                bool isDebug = ev.Level == LogEventLevel.Debug;
                if (isDebug) { return true; }
                return false;
            }, wt => wt.Console());
        })
        .WriteTo.Logger(l =>
        {
            l.WriteTo.Conditional(ev =>
            {
                bool isDebug = ev.Level == LogEventLevel.Debug;
                if (isDebug) { return true; }
                return false;
            }, wt => wt.File(
                    path,
                    rollingInterval: RollingInterval.Day));
        })
        .WriteTo.Logger(l =>
        {
            l.WriteTo.Conditional(ev =>
            {
                bool isInformation = ev.Level == LogEventLevel.Information;
                if (isInformation) { return true; }
                return false;
            },
            wt => wt.MSSqlServer(
                    connectionString: dbConnection,
                    sinkOptions: sinkOptsHead,
                    columnOptions: columnOptsHead));
        })
        .WriteTo.Logger(l =>
        {
            l.WriteTo.Conditional(ev =>
            {
                bool isWarning = ev.Level == LogEventLevel.Warning;
                if (isWarning) { return true; }
                return false;
            },
            wt => wt.MSSqlServer(
                    connectionString: dbConnection,
                    sinkOptions: sinkOptsDetails,
                    columnOptions: columnOptsDetails));
        })
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
