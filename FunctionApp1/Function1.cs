using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public class Function1
    {
        [Function("Function1")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req, FunctionContext context)
        {
            //_logger.LogInformation("C# HTTP trigger function processed a request.");
            var logger = context.GetLogger<Function1>();
            logger.LogDebug("C# HTTP trigger function processed a request.");
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");
            string useCase = "New Order";
            string orderStatus = "Received";
            var guid = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var service = req.Url;
            string levelType = "HttpRequest";

            logger.LogInformation("{GLOBAL_UUID}{USE_CASE}{ORDER_STATUS}{TIMESTAMP}", guid, useCase, orderStatus, DateTime.Now);
            logger.LogWarning("{GLOBAL_UUID}{USE_CASE}{REQUEST_UUID}{SERVICE}{END_SYSTEM}{STATE}{PAYLOAD}{TIMESTAMP}{LEVEL_TYPE}", 
                                guid, useCase, guid2, service, context.GetType, orderStatus, req.Body, DateTime.Now, levelType);

            string utcDate = DateTime.UtcNow.AddHours(3).ToString();
            logger.LogDebug(utcDate);

            return response;
        }
    }
}
