using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

// match is by name and argument count, parameter types not considered
// so you can't have two commands with the same name and the same number of arguments
//
// eg this is not valid:
//
// [Help("NOPE1")] void foo(string s) {}
// [Help("NOPE2")] void foo(int n) {}
//
// if you do this, only the first command will ever get called
//
// if a command is not found, a UsageError is thrown
// if a command has the wrong # of arguments, a UsageError is thrown
// Commands can throw a SilentExit exception to terminate the application silently with a return code of 0
// Commands can throw a FatalError exception to terminate the application with an error message and a return code of 1
// if a FatalError is thrown, the message will be printed before the application exits
// if no command is found to run, the first command found with a [Default] attribute will be run

namespace Args
{
    //////////////////////////////////////////////////////////////////////

    static class Print
    {
        static void InternalPrint(TextWriter t, string m, ConsoleColor c)
        {
            var old_color = Console.ForegroundColor;
            Console.ForegroundColor = c;
            t.Write(m);
            Console.ForegroundColor = old_color;
        }

        static void InternalPrintLine(TextWriter t, string m, ConsoleColor c)
        {
            InternalPrint(t, $"{m}\n", c);
        }

        public static void Error(string m)
        {
            InternalPrintLine(Console.Error, m, ConsoleColor.Red);
        }

        public static void Warning(string m)
        {
            InternalPrintLine(Console.Out, m, ConsoleColor.Yellow);
        }

        public static void Line(string m, ConsoleColor c = ConsoleColor.White)
        {
            InternalPrintLine(Console.Out, m, c);
        }

        public static void Text(string m, ConsoleColor c = ConsoleColor.White)
        {
            InternalPrint(Console.Out, m, c);
        }
    }

    //////////////////////////////////////////////////////////////////////

    static class Helper
    {
        public static bool IsCommandMethod(this MethodInfo m)
        {
            return m.IsDefined(typeof(HelpAttribute));
        }
    }

    //////////////////////////////////////////////////////////////////////
    // apply to a function which should be callable via command line

    public class HelpAttribute: Attribute
    {
        public string help { get; set; }

        public HelpAttribute(string help_text)
        {
            help = help_text;
        }
    }

    //////////////////////////////////////////////////////////////////////
    // apply to a function which should be called when no other function is called

    public class DefaultAttribute: Attribute
    {
    }

    //////////////////////////////////////////////////////////////////////
    // base Error class (FatalError, UsageError, SilentError)

    public class Error: ApplicationException
    {
        public Error() : base()
        {
        }

        public Error(string reason) : base(reason)
        {
        }
    }

    //////////////////////////////////////////////////////////////////////

    public class UsageError: Error
    {
        public UsageError(string reason) : base(reason)
        {
        }
    }

    //////////////////////////////////////////////////////////////////////

    public class FatalError: Error
    {
        public int return_code;

        public FatalError(string reason, int return_code) : base(reason)
        {
            this.return_code = return_code;
        }
    }

    //////////////////////////////////////////////////////////////////////

    public class SilentError: Error
    {
        public SilentError() : base()
        {
        }
    }

    //////////////////////////////////////////////////////////////////////
    // Derive your class from this and add handlers with [Help("helptext")] attribute

    public class Handler
    {
        const BindingFlags binding_flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public;

        //////////////////////////////////////////////////////////////////////

        private string param_help(MethodInfo method)
        {
            StringBuilder s = new StringBuilder();
            if(method.IsDefined(typeof(HelpAttribute)))
            {
                foreach(ParameterInfo param in method.GetParameters())
                {
                    Type t = param.ParameterType;
                    string n = param.Name;
                    if(t == typeof(string))
                    {
                        s.Append($"{param.Name} ");
                    }
                    else if(t.IsEnum)
                    {
                        s.Append($"[{string.Join("|", Enum.GetNames(t))}] ".ToLower());
                    }
                    else if(t.IsPrimitive)
                    {
                        s.Append($"{n}({t.Name.ToLower()}) ");
                    }
                    else
                    {
                        s.Append("?? ");
                    }
                }
            }
            return s.ToString();
        }

        //////////////////////////////////////////////////////////////////////
        // get the help string for a method (or null)

        private string get_help(MethodInfo method)
        {
            return method.GetCustomAttribute<HelpAttribute>()?.help;
        }

        //////////////////////////////////////////////////////////////////////
        // run a method after parsing its arguments

        private void run(MethodInfo method, string[] args, bool run_it)
        {
            ParameterInfo[] params_info = method.GetParameters();

            // this should never happen, arg counts checked in parse_args
            if(args.Length != params_info.Length + 1)
            {
                throw new UsageError($"{method.Name} command needs {params_info.Length} parameters, only got {args.Length}");
            }
            object[] result = new object[params_info.Length];
            for(int i = 0; i<params_info.Length; ++i)
            {
                Type param_type = params_info[i].ParameterType;
                string arg_str = args[i + 1].ToLower();

                // strings passed through
                if(param_type == typeof(string))
                {
                    result[i] = arg_str;
                }

                // enums get parsed using Enum.Parse()
                else if(param_type.IsEnum)
                {
                    if(!Enum.GetNames(param_type).Any(x => x.ToLower() == arg_str))
                    {
                        throw new UsageError($"Error, unknown option '{arg_str}' for {method.Name}");
                    }
                    result[i] = Enum.Parse(param_type, arg_str, ignoreCase: true);
                }

                // probly a number of some sort, give Parse() a go
                else if(param_type.IsPrimitive)
                {
                    MethodInfo parse_method = param_type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                    if(parse_method != null)
                    {
                        try
                        {
                            result[i] = parse_method.Invoke(null, new object[] { arg_str });
                        }
                        catch(TargetInvocationException e) when(e.InnerException.GetType() == typeof(FormatException))
                        {
                            throw new UsageError($"Error, can't parse {arg_str} as {param_type.Name}: {e.InnerException.Message}");
                        }
                        catch(TargetInvocationException e) when(e.InnerException.GetType() == typeof(OverflowException))
                        {
                            long min_val;
                            ulong max_val;
                            // get MaxValue and MinValue
                            FieldInfo max_value = param_type.GetField("MaxValue");
                            FieldInfo min_value = param_type.GetField("MinValue");
                            max_val = (ulong)Convert.ChangeType(max_value.GetValue(null), typeof(ulong));
                            min_val = (long)Convert.ChangeType(min_value.GetValue(null), typeof(long));
                            throw new UsageError($"Error, value {arg_str} out of range for {parse_method.Name} (can be {min_val}..{max_val})");
                        }
                    }
                    else
                    {
                        throw new FatalError($"Error, can't find {param_type.Name}.Parse(string)", -1);
                    }
                }

                // it can't handle any complex parameter types (yet? ever?)
                else
                {
                    throw new FatalError($"Error, don't know how to parse a {param_type.Name}", -2);
                }
            }
            if(run_it)
            {
                try
                {
                    method.Invoke(this, result);
                }
                catch(TargetInvocationException e)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
            }
        }

        //////////////////////////////////////////////////////////////////////
        // find all the command methods (those with a [Help()] attribute

        private IEnumerable<MethodInfo> CommandMethods
        {
            get
            {
                return GetType().GetMethods(binding_flags).Where(x => x.IsDefined(typeof(HelpAttribute)));
            }
        }

        //////////////////////////////////////////////////////////////////////
        // get the Default method (called when no args are supplied)

        private MethodInfo DefaultMethod
        {
            get
            {
                return GetType().GetMethods(binding_flags).First(x => x.IsDefined(typeof(DefaultAttribute)));
            }
        }

        //////////////////////////////////////////////////////////////////////
        // scan args for a command to run or just check usage

        private bool parse_args(string[] command_line, bool run_commands)
        {
            int functions_called = 0;
            string s = string.Join(" ", command_line);
            char[] semicolon = new char[] { ';' };
            char[] whitespace = new char[] { ' ', '\t' };
            string[] p = s.Split(semicolon, StringSplitOptions.RemoveEmptyEntries);
            foreach(string a in p)
            {
                string[] argv = a.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
                if(argv.Length != 0)
                {
                    List<int> param_counts = new List<int>();
                    bool name_match = false;
                    bool args_match = false;

                    foreach(MethodInfo method in CommandMethods.Where(x => string.Compare(argv[0], x.Name, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        name_match = true;
                        ParameterInfo[] parameters = method.GetParameters();
                        param_counts.Add(parameters.Length);
                        if(parameters.Length == argv.Length - 1)
                        {
                            args_match = true;
                            functions_called += 1;
                            run(method, argv, run_it: run_commands);
                        }
                    }
                    if(!name_match)
                    {
                        throw new UsageError($"Unknown command {argv[0]}");
                    }
                    if(!args_match)
                    {
                        throw new UsageError($"Wrong number of arguments for {argv[0]}, expected {string.Join(" or ", param_counts)}");
                    }
                }
            }
            if(functions_called == 0 && run_commands)
            {
                MethodInfo m = DefaultMethod;
                if(m != null)
                {
                    try
                    {
                        m.Invoke(this, null);
                    }
                    catch(TargetInvocationException e)
                    {
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    }
                }
            }
            return true;
        }

        //////////////////////////////////////////////////////////////////////
        // execute handlers according to command line arguments

        public bool execute(string[] args)
        {
            try
            {
                parse_args(args, run_commands: false);
            }
            catch(UsageError e)
            {
                Print.Error($"{e.Message}");
                return false;
            }
            try
            {
                parse_args(args, run_commands: true);
            }
            catch(FatalError e)
            {
                Print.Error(e.Message);
                Environment.ExitCode = e.return_code;
            }
            catch(SilentError)
            {
            }
            return true;
        }

        //////////////////////////////////////////////////////////////////////
        // show help for all commands

        public void show_help(string application_name, string header)
        {
            string exe = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName).ToLower();
            Print.Line($"{application_name}\n", ConsoleColor.Green);
            Print.Line($"Usage: {exe} command [params]; command [params]; etc\n");
            Print.Line($"{header}\n");
            Print.Line("Commands:\n");

            int name_len = 0;
            int help_len = 0;
            int param_len = 0;

            foreach(MethodInfo m in CommandMethods)
            {
                name_len = Math.Max(m.Name.Length, name_len);
                help_len = Math.Max(get_help(m).Length, help_len);
                param_len = Math.Max(param_help(m).Length, param_len);
            }

            foreach(MethodInfo m in CommandMethods)
            {
                Print.Text($"{m.Name.PadRight(name_len)}", ConsoleColor.Yellow);
                Print.Line($" {param_help(m).PadRight(param_len)} {get_help(m).PadRight(help_len)}");
            }
            Print.Line("");
        }
    }
}
