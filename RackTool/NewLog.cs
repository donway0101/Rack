using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;


namespace RackTool
{
    public  class NewLog
    {
        private static readonly string log_config = 
            System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "NLog.config";
        private static Logger _instance = null;
        private NewLog() { }
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    init ();
                }
                
                return _instance;
            }
        }
        public static void init ()
        {
            LogManager.Configuration = new XmlLoggingConfiguration (log_config);
            _instance = LogManager.GetCurrentClassLogger();
        }
    }
}
