using MelonLoader;
using System.Reflection;

namespace MapDLLInjector
{
    public static class Logger
    {
        static MelonLogger.Instance loggerInstance = new MelonLogger.Instance(Assembly.GetExecutingAssembly().GetName().Name, System.ConsoleColor.Green);
        static MelonLogger.Instance errorLoggerInstance = new MelonLogger.Instance(Assembly.GetExecutingAssembly().GetName().Name, System.ConsoleColor.Red);
        public static void Log(string msg)
        {
            loggerInstance.Msg(msg);
        }

        public static void LogError(string err)
        {
            errorLoggerInstance.Msg(err);
        }
    }
}
