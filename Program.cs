//////////////////////////////////////////////////////////////////////

using McMaster.Extensions.CommandLineUtils;
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
            CommandOption verbose_option = app.Option("-v|--verbose", "verbosity (0..4), defaults to 2", CommandOptionType.SingleOrNoValue);

            app.OnExecute(() =>
            {
                bool ok = true;

                // verbose
                Log.Level verbosity = Log.Level.Info;
                if (verbose_option.HasValue())
                {
                    Log.level = Log.Level.Verbose;
                    if (verbose_option.Value() != null)
                    {
                        if (Enum.TryParse(verbose_option.Value(), true, out Log.Level level) && level >= 0 && level <= Log.Level.Error)
                        {
                            verbosity = level;
                        }
                        else if (int.TryParse(verbose_option.Value(), out int level_int) && level_int >= 0 && level_int <= (int)Log.Level.Error)
                        {
                            verbosity = (Log.Level)level_int;
                        }
                        else
                        {
                            Log.Error($"Invalid verbosity level '{verbose_option.Value()}'");
                            ok = false;
                        }
                    }
                }
                Log.level = verbosity;

                // com_port
                if (!(com_port_option.HasValue() && address_option.HasValue() && baud_rate_option.HasValue()))
                {
                    Log.Error("Need at least com port, baud rate and address");
                    app.ShowHelp();
                    ok = false;
                }

                // address
                byte address;
                if (!byte.TryParse(address_option.Value(), out address))
                {
                    Log.Error($"Invalid device address: {address_option.Value()}");
                    app.ShowHelp();
                    ok = false;
                }

                // baud_rate
                int baud_rate;
                if (!int.TryParse(baud_rate_option.Value(), out baud_rate))
                {
                    Log.Error($"Invalid baud rate: {baud_rate_option.Value()}");
                    app.ShowHelp();
                    ok = false;
                }

                // if from, to, interval, step are specified, do that, otherwise just show status
                int from = 0;
                int to = 0;
                int interval = 0;
                int step = 0;
                bool ramp = from_option.HasValue() && to_option.HasValue() && interval_option.HasValue() && step_option.HasValue();
                if (!ramp)
                {
                    if (from_option.HasValue() || to_option.HasValue() || interval_option.HasValue() || step_option.HasValue())
                    {
                        Log.Error("Must specify all or none of --from, --to, --interval, --step options");
                        ok = false;
                    }
                }
                else
                {
                    // validate from, to, step, interval parameters
                    if (!int.TryParse(from_option.Value(), out from) || from < 0 || from > 40000)
                    {
                        Log.Error($"Bad value ({from_option.Value()}) for 'from' option, must be 0 .. 40000 (mA)");
                        ok = false;
                    }
                    if (!int.TryParse(to_option.Value(), out to) || to < 0 || to > 40000)
                    {
                        Log.Error($"Bad value ({to_option.Value()})for 'to' option, must be 0 .. 40000 (mA)");
                        ok = false;
                    }
                    if (!int.TryParse(interval_option.Value(), out interval) || interval < 0 || interval > 360000)
                    {
                        Log.Error($"Bad value ({interval_option.Value()})for 'interval' option, must be 0 .. 360000 (ms)");
                        ok = false;
                    }
                    if (!int.TryParse(step_option.Value(), out step) || Math.Abs(step) > 10000)
                    {
                        Log.Error($"Bad ({step_option.Value()})value for 'step' option, must be 0 .. 10000 (mA)");
                        ok = false;
                    }
                    if (from == to)
                    {
                        Log.Error($"Can't step from {from} to {to}");
                        ok = false;
                    }
                    if (step == 0)
                    {
                        Log.Error($"Can't step 0, it will never get there");
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
                Log.Verbose($"COM Port: {com_port_option.Value()}");
                Log.Verbose($"BAUD Rate: {baud_rate}");
                Log.Verbose($"Address: {address}");
                device.address = address;
                device.get_status();
                if (ramp)
                {
                    if ((from < to && step < 0) || (from > to && step > 0))
                    {
                        step = -step;
                        Log.Warning($"Changing step to {step} from {-step} so it works");
                    }
                    Log.Info($"Stepping from {from} to {to} in steps of {step}mA at intervals of {interval}ms");
                    int current = from;
                    var stop_watch = new System.Diagnostics.Stopwatch();
                    var step_time = TimeSpan.FromMilliseconds(step);
                    while ((step < 0 && current >= to) || (step > 0 && current <= to))
                    {
                        stop_watch.Restart();
                        device.set_current((uint)current);
                        current += step;
                        stop_watch.Stop();
                        int ms_remaining = (step_time - stop_watch.Elapsed).Milliseconds;
                        if (ms_remaining > 0)
                        {
                            Thread.Sleep(ms_remaining);
                        }
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
                Log.Error($"{e.Message}");
            }
        }
    }
}
