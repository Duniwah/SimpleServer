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
            LogManager.GetLogger(typeof(Debug));
        }
    }
}
