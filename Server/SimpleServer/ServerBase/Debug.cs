using log4net;
using log4net.Config;
using System;
using System.IO;
namespace ServerBase
{
    public static class Debug
    {
        private static ILog _log;
        static Debug()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));
            _log = LogManager.GetLogger(typeof(Debug));
        }

        public static void Log(object message)
        {
            _log.Debug(message);
        }

        public static void Log(string format, params object[] args)
        {
            _log.DebugFormat(format, args);
        }

        public static void LogInfo(object message)
        {
            _log.Info(message);
        }

        public static void LogInfo(string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }

        public static void LogWarn(object message)
        {
            _log.Warn(message);
        }
        
        public static void LogWarn(string format, params object[] args)
        {
            _log.WarnFormat(format, args);
        }
        
        public static void LogError(object message)
        {
            _log.Error(message);
        }
        
        public static void LogError(string format, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }
        
        public static void LogFatal(object message)
        {
            _log.Fatal(message);
        }
        
        public static void LogFatal(string format, params object[] args)
        {
            _log.FatalFormat(format, args);
        }
    }
}
