using System;
using System.Runtime.CompilerServices;

namespace KP184
{
    class Log
    {
        public enum Level
        {
            Debug,
            Verbose,
            Info,
            Warning,
            Error
        };

        static Level _log_level = Level.Info;

        public static Level level
        {
            get
            {
                return _log_level;
            }
            set
            {
                _log_level = value;
                Log.Debug($"Setting log level to {value}");
            }
        }

        public static void Write(Level severity, object o, ConsoleColor color, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            if(severity >= level)
            {
                string now = DateTime.Now.ToString("HH:mm:ss.fff");
                string text = $"{severity,-8} {now} {tag,-20}:{o}";
                System.Diagnostics.Debug.WriteLine(text);
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }

        public static void Debug(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Debug, o, ConsoleColor.DarkGreen, tag, file, line);
        }
        public static void Verbose(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Verbose, o, ConsoleColor.Cyan, tag, file, line);
        }
        public static void Info(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Info, o, ConsoleColor.White, tag, file, line);
        }
        public static void Warning(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Warning, o, ConsoleColor.Yellow, tag, file, line);
        }
        public static void Error(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Error, o, ConsoleColor.Red, tag, file, line);
        }
    }
}
