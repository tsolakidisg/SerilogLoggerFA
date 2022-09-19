using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogTestFA
{
    public class LoggerHelper
    {
        public static void HeadTableLogging(Guid global_uuid, string use_case, string order_status)
        {
            var loggerH = LogManager.GetLogger("Head_Table");

            LogEventInfo headLogEvent = new LogEventInfo(LogLevel.Info, "Head_Table", "");
            headLogEvent.Properties.Add("GlobalUuid", global_uuid);
            headLogEvent.Properties.Add("UseCase", use_case);
            headLogEvent.Properties.Add("OrderStatus", order_status);

            loggerH.Info(headLogEvent);
        }

        public static void DetailsTableLogging(Guid global_uuid, Guid request_uuid, string use_case, string service, string end_system, string state, object payload, object ex)
        {
            var loggerD = LogManager.GetLogger("Details_Table");

            if (state == "Error")
            {
                LogEventInfo detailsLogEvent = new LogEventInfo(LogLevel.Error, "Details_Table", null, "", new List<object>().ToArray(), (Exception)ex);
                detailsLogEvent.Properties.Add("GlobalUuid", global_uuid);
                detailsLogEvent.Properties.Add("UseCase", use_case);
                detailsLogEvent.Properties.Add("RequestUuid", request_uuid);
                detailsLogEvent.Properties.Add("Service", service);
                detailsLogEvent.Properties.Add("EndSystem", end_system);
                detailsLogEvent.Properties.Add("State", state);
                detailsLogEvent.Properties.Add("Payload", "");
                loggerD.Log(detailsLogEvent);
            }
            else
            {
                LogEventInfo detailsLogEvent = new LogEventInfo(LogLevel.Info, "Details_Table", "");
                detailsLogEvent.Properties.Add("GlobalUuid", global_uuid);
                detailsLogEvent.Properties.Add("UseCase", use_case);
                detailsLogEvent.Properties.Add("RequestUuid", request_uuid);
                detailsLogEvent.Properties.Add("Service", service);
                detailsLogEvent.Properties.Add("EndSystem", end_system);
                detailsLogEvent.Properties.Add("State", state);
                detailsLogEvent.Properties.Add("Payload", payload);
                loggerD.Info(detailsLogEvent);
            }
        }
    }
}
