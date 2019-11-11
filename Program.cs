//////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Threading;

//////////////////////////////////////////////////////////////////////

namespace modbus
{
    //////////////////////////////////////////////////////////////////////

    class Program
    {
        static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication(false)
            {
                Name = "KP184",
                Description = "Control the KP184"
            };

            app.HelpOption("--help");

            CommandOption com_port_option = app.Option("-p|--com", "com port (string)", CommandOptionType.SingleValue);
            CommandOption address_option = app.Option("-a|--address", "device address (byte)", CommandOptionType.SingleValue);
            CommandOption baud_rate_option = app.Option("-b|--baud", "baud rate", CommandOptionType.SingleValue);
            CommandOption from_option = app.Option("-f|--from", "starting current (mA)", CommandOptionType.SingleValue);
            CommandOption to_option = app.Option("-t|--to", "ending current (mA)", CommandOptionType.SingleValue);
            CommandOption interval_option = app.Option("-i|--interval", "interval time(ms)", CommandOptionType.SingleValue);
            CommandOption step_option = app.Option("-s|--step", "step amount (mA)", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                bool ok = true;
                if (!(com_port_option.HasValue() && address_option.HasValue() && baud_rate_option.HasValue()))
                {
                    Console.Error.WriteLine("Need at least com port, baud rate and address");
                    app.ShowHelp();
                    ok = false;
                }
                byte address;
                if (!byte.TryParse(address_option.Value(), out address))
                {
                    Console.Error.WriteLine($"Invalid device address: {address_option.Value()}");
                    app.ShowHelp();
                    ok = false;
                }
                int baud_rate;
                if(!int.TryParse(baud_rate_option.Value(), out baud_rate))
                {
                    Console.Error.WriteLine($"Invalid baud rate: {baud_rate_option.Value()}");
                    app.ShowHelp();
                    ok = false;
                }
                // if from, to, interval, step are specified, do that, otherwise just show status
                int from = 0;
                int to = 0;
                int interval = 0;
                int step = 0;
                bool ramp = from_option.HasValue() && to_option.HasValue() && interval_option.HasValue() && step_option.HasValue();
                if(!ramp)
                {
                    if (from_option.HasValue() || to_option.HasValue() || interval_option.HasValue() || step_option.HasValue())
                    {
                        Console.Error.WriteLine("Must specify all or none of --from, --to, --interval, --step options");
                        ok = false;
                    }
                }
                else
                {
                    if (!int.TryParse(from_option.Value(), out from) || from < 0 || from > 40000)
                    {
                        Console.Error.WriteLine($"Bad value for 'from' option, must be 0 .. 40000 (mA)");
                        ok = false;
                    }
                    if (!int.TryParse(to_option.Value(), out to) || to < 0 || to > 40000)
                    {
                        Console.Error.WriteLine($"Bad value for 'to' option, must be 0 .. 40000 (mA)");
                        ok = false;
                    }
                    if (!int.TryParse(interval_option.Value(), out interval) || interval < 0 || interval > 360000)
                    {
                        Console.Error.WriteLine($"Bad value for 'interval' option, must be 0 .. 360000 (ms)");
                        ok = false;
                    }
                    if (!int.TryParse(step_option.Value(), out step) || Math.Abs(step) > 10000)
                    {
                        Console.Error.WriteLine($"Bad value for 'step' option, must be 0 .. 10000 (mA)");
                        ok = false;
                    }
                    if (from == to)
                    {
                        Console.Error.WriteLine($"Can't step from {from} to {to}");
                        ok = false;
                    }
                    if(step == 0)
                    {
                        Console.Error.WriteLine($"Can't step 0, it will never get there");
                        ok = false;
                    }
                }
                if (!ok)
                {
                    return 0;
                }
                kp184 device = new kp184();
                if (!device.open(com_port_option.Value(), baud_rate))
                {
                    return 0;
                }
                Console.WriteLine($"COM Port: {com_port_option.Value()}");
                Console.WriteLine($"BAUD Rate: {baud_rate}");
                Console.WriteLine($"Address: {address}");
                device.address = address;
                device.get_status();
                if(ramp)
                {
                    if((from < to && step < 0) || (from > to && step > 0))
                    {
                        step = -step;
                        Console.WriteLine($"Changing stop to {step} from {-step} so it works");
                    }
                    Console.WriteLine($"Stepping from {from} to {to} in steps of {step}mA at intervals of {interval}ms");
                    int current = from;
                    while((step < 0 && current >= to) || (step > 0 && current <= to))
                    {
                        device.set_current((uint)current);
                        current += step;
                        Thread.Sleep(interval);
                    }
                }
                device.close();
                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                Console.Error.WriteLine($"{e.Message}");
            }
        }
    }
}
