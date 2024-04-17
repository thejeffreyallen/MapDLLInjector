using MelonLoader;
using System.Reflection;

namespace MapDLLInjector
{
    /// <summary>
    /// Provides a utility class for logging normal and error messages with color differentiation.
    /// </summary>
    /// <remarks>
    /// This class utilizes the MelonLogger framework to log messages. There are two loggers instantiated:
    /// one for normal messages, displayed in green, and one for error messages, displayed in red.
    /// Each logger instance is associated with the name of the currently executing assembly.
    /// The class provides static methods to log standard messages and error messages.
    /// </remarks>
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
