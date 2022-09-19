using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    //.ConfigureLogging((context, logging) =>
    //{
    //    logging.ClearProviders();
    //    logging.AddProvider();
    //    logging.AddConsole();
    //    logging.AddDebug();
    //})
    .Build();

host.Run();
