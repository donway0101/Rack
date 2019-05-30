using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace RackTool
{
    public static class NewLog
    {
        private static readonly string log_config =
            System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "NLog.config";
        private readonly static ILogger _instance = null;
        static NewLog()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("levelx", typeof(LevelExlayoutRenderer));
            _instance = LogManager.GetCurrentClassLogger();
        }
        public static ILogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    init();
                }
                return _instance;
            }
        }
        public static void init()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(log_config);
            //_instance = LogManager.GetCurrentClassLogger();
        }
    }
    public static class NlogExtend
    {
        public static void Ng(this ILogger logger, string message1 = "Null", string message2 = "Null", string message3 = "Null", string message4 = "Null")
        {
            string message = message1 + " " + message2 + " " + message3 + " " + message4;
            var LogEventInfo = new LogEventInfo(LogLevel.Trace, logger.Name, message);
            LogEventInfo.Properties["custLevel"] = Tuple.Create(9, "Ng");
            logger.Log(LogEventInfo);
        }
        public static void Pass(this ILogger logger, string message1 = "Null", string message2 = "Null", string message3 = "Null", string message4 = "Null")
        {
            string message = message1 + " " + message2 + " " + message3 + " " + message4;
            var LogEventInfo = new LogEventInfo(LogLevel.Trace, logger.Name, message);
            LogEventInfo.Properties["custLevel"] = Tuple.Create(9, "Pass");
            logger.Log(LogEventInfo);
        }
    }
    [LayoutRenderer("levelx")]
    public class LevelExlayoutRenderer : LevelLayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.Level == LogLevel.Trace && logEvent.Properties.ContainsKey("custLevel"))
            {
                var custLevel = logEvent.Properties["custLevel"] as Tuple<int, string>;
                if (custLevel == null)
                {
                    throw new InvalidCastException("Invalid Properties[\"custLevel\"] as Tuple<int, string>;");
                }
                switch (this.Format)
                {
                    case LevelFormat.Name:
                        builder.Append(custLevel.Item2);
                        break;
                    case LevelFormat.FirstCharacter:
                        builder.Append(custLevel.Item2[0]);
                        break;
                    case LevelFormat.Ordinal:
                        builder.Append(custLevel.Item1);
                        break;
                    default: break;
                }
            }
            else
            {
                base.Append(builder, logEvent);
            }
        }
    }
}
