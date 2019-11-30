using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Args
{
    //////////////////////////////////////////////////////////////////////

    static class Print
    {
        static void InternalPrint(string m, ConsoleColor c)
        {
            var old_color = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.Error.WriteLine(m);
            Console.ForegroundColor = old_color;
        }

        public static void Error(string m)
        {
            InternalPrint(m, ConsoleColor.Red);
        }

        public static void Warning(string m)
        {
            InternalPrint(m, ConsoleColor.Yellow);
        }

        public static void Message(string m)
        {
            Console.WriteLine(m);
        }
    }

    //////////////////////////////////////////////////////////////////////

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

    public class DefaultAttribute : Attribute
    {
    }

    //////////////////////////////////////////////////////////////////////

    public class Error: ApplicationException
    {
        public Error(string reason) : base(reason)
        {
        }
    }

    //////////////////////////////////////////////////////////////////////

    public class FatalError: ApplicationException
    {
        public FatalError(string reason) : base(reason)
        {
        }
    }

    //////////////////////////////////////////////////////////////////////

    public class Handler
    {
        const BindingFlags binding_flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public;

        //////////////////////////////////////////////////////////////////////

        string param_help(MethodInfo method)
        {
            ParameterInfo[] p = method.GetParameters();
            StringBuilder s = new StringBuilder();
            foreach(HelpAttribute h in method.GetCustomAttributes<HelpAttribute>())
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

        string get_help(MethodInfo method)
        {
            return method.GetCustomAttribute<HelpAttribute>()?.help;
        }

        //////////////////////////////////////////////////////////////////////

        public bool run(MethodInfo method, string[] args)
        {
            ParameterInfo[] p = method.GetParameters();
            if(args.Length != p.Length + 1)
            {
                Print.Error($"{method.Name} command needs {p.Length} parameters, only got {args.Length}");
                return false;
            }
            object[] result = new object[p.Length];
            for(int i = 0; i<p.Length; ++i)
            {
                Type t = p[i].ParameterType;
                string s = args[i + 1].ToLower();

                // strings passed through
                if(t == typeof(string))
                {
                    result[i] = s;
                }

                // enums get parsed using Enum.Parse()
                else if(t.IsEnum)
                {
                    if(!Enum.GetNames(t).Any(x => x.ToLower() == s))
                    {
                        Print.Error($"Error, unknown option '{s}' for {method.Name}");
                        return false;
                    }
                    result[i] = Enum.Parse(t, s, true);
                }

                // probly a number of some sort, give Parse() a go
                else if(t.IsPrimitive)
                {
                    MethodInfo m = t.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                    if(m != null)
                    {
                        try
                        {
                            result[i] = m.Invoke(null, new object[] { s });
                        }
                        catch(TargetInvocationException e) when(e.InnerException.GetType() == typeof(FormatException))
                        {
                            Print.Error($"Error, can't parse {s} as {t.Name}: {e.InnerException.Message}");
                            return false;
                        }
                        catch(TargetInvocationException e) when(e.InnerException.GetType() == typeof(OverflowException))
                        {
                            long min_val;
                            ulong max_val;
                            // get MaxValue and MinValue
                            FieldInfo max_value = t.GetField("MaxValue");
                            FieldInfo min_value = t.GetField("MinValue");
                            max_val = (ulong)Convert.ChangeType(max_value.GetValue(null), typeof(ulong));
                            min_val = (long)Convert.ChangeType(min_value.GetValue(null), typeof(long));
                            Print.Error($"Error, value {s} out of range for {method.Name} (can be {min_val}..{max_val})");
                            return false;
                        }
                    }
                    else
                    {
                        Print.Error($"Error, can't find {t.Name}.Parse(string)");
                        return false;
                    }
                }

                // it can't handle any complex parameter types (yet? ever?)
                else
                {
                    Print.Error($"Error, don't know how to parse a {t.Name}");
                    return false;
                }
            }
            try
            {
                method.Invoke(this, result);
            }
            catch(TargetInvocationException e) when(e.InnerException.GetType() == typeof(Args.Error))
            {
                if(!string.IsNullOrEmpty(e.InnerException.Message))
                {
                    Print.Error($"Error processing argument for {method.Name.ToLower()}: {e.InnerException.Message}");
                }
                return false;
            }
            catch(TargetInvocationException e)
            {
                throw e.InnerException;
            }
            return true;
        }

        //////////////////////////////////////////////////////////////////////

        public bool execute(string[] args)
        {
            MethodInfo[] methods = GetType().GetMethods(binding_flags);
            int functions_called = 0;
            string s = string.Join(" ", args);
            string[] p = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string a in p)
            {
                string[] r = a.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if(r.Length != 0)
                {
                    List<int> parameter_counts = new List<int>();
                    bool found = false;
                    string found_command = null;
                    foreach(MethodInfo method in methods)
                    {
                        if(string.Compare(r[0], method.Name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            found_command = method.Name.ToLower();
                            ParameterInfo[] parameters = method.GetParameters();
                            if(method.GetCustomAttribute<HelpAttribute>() != null)
                            {
                                parameter_counts.Add(parameters.Length);
                                if (parameters.Length == r.Length - 1)
                                {
                                    found = true;
                                    functions_called += 1;
                                    if (run(method, r))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if(found_command == null)
                    {
                        Print.Warning($"Warning: unknown command {r[0]} ignored");
                    }
                    else if(!found)
                    {
                        string param_counts = "0";
                        if(parameter_counts.Count != 0)
                        {
                            param_counts = string.Join("or", parameter_counts.ToArray());
                        }
                        Print.Error($"Wrong number of arguments for {found_command}, expected {param_counts}");
                    }
                }
            }
            if(functions_called == 0)
            {
                foreach(MethodInfo m in GetType().GetMethods(binding_flags))
                {
                    if(m.GetCustomAttribute<DefaultAttribute>() != null && m.GetParameters().Length == 0)
                    {
                        try
                        {
                            m.Invoke(this, null);
                        }
                        catch(TargetInvocationException e) when (e.InnerException.GetType().BaseType == typeof(System.ApplicationException))
                        {
                        }
                        break;
                    }
                }

            }
            return true;
        }

        public static bool find_default(MemberInfo m, Object s)
        {
            return m.GetCustomAttribute<DefaultAttribute>() != null;
        }

        //////////////////////////////////////////////////////////////////////

        public void show_help(string header)
        {
            int name_len = 0;
            int help_len = 0;
            int param_len = 0;

            foreach(MethodInfo m in GetType().GetMethods(binding_flags))
            {
                name_len = Math.Max(m.Name.Length, name_len);
                help_len = Math.Max(get_help(m).Length, help_len);
                param_len = Math.Max(param_help(m).Length, param_len);
            }

            Console.WriteLine($"{header}\n\nCommands:\n");
            foreach(MethodInfo m in GetType().GetMethods(binding_flags))
            {
                Console.WriteLine($"{m.Name.PadRight(name_len)} {param_help(m).PadRight(param_len)} {get_help(m).PadRight(help_len)}");
            }
            Console.WriteLine();
        }
    }
}
