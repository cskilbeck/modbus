﻿using System;
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

        public static Level level = Level.Info;

        public static void Write(Level severity, object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            if(severity >= level)
            {
                System.Diagnostics.Debug.WriteLine($"{tag}:{o}");
                Console.WriteLine($"{tag}:{o}");
            }
        }

        public static void Debug(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Debug, o, tag, file, line);
        }
        public static void Verbose(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Verbose, o, tag, file, line);
        }
        public static void Info(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Info, o, tag, file, line);
        }
        public static void Warning(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Warning, o, tag, file, line);
        }
        public static void Error(object o, [CallerMemberName]string tag = "", [CallerFilePath]string file = "", [CallerLineNumber]int line = 0)
        {
            Write(Level.Error, o, tag, file, line);
        }
    }
}
