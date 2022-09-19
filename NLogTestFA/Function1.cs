using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace NLogTestFA
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var guid = Guid.NewGuid();
            var coreApiUuid = Guid.NewGuid();
            var soapUuid = Guid.NewGuid();
            // Create a JSON string to transform the XML object into JSON
            string JSONString = string.Empty;

            LoggerHelper.HeadTableLogging(guid, "New Order", "New");

            LoggerHelper.DetailsTableLogging(guid, coreApiUuid, "New Order", "Orders/id", "Core API", "Initial", "{'Id':" + Guid.NewGuid() + "}", new object() { });
            // Create service object to connect in the SOAP WS using WSDL
            LoggerHelper.DetailsTableLogging(guid, soapUuid, "New Order", "SoapDemo.asmx?op=GetOrderStatus", "SOAP_WS", "Sent", "{'Id':" + Guid.NewGuid() + "}", new object() { });


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
