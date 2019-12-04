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

        public static void Write(Level severity, object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            if(severity >= level)
            {
                System.Diagnostics.Debug.WriteLine($"{tag}:{o}");
                Console.WriteLine($"{tag,-20}:{o}");
            }
        }

        public static void Debug(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Write(Level.Debug, o, tag, file, line);
            Console.ResetColor();
        }
        public static void Verbose(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Write(Level.Verbose, o, tag, file, line);
            Console.ResetColor();
        }
        public static void Info(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Info, o, tag, file, line);
            Console.ResetColor();
        }
        public static void Warning(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write(Level.Warning, o, tag, file, line);
            Console.ResetColor();
        }
        public static void Error(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(Level.Error, o, tag, file, line);
            Console.ResetColor();
        }
    }
}
