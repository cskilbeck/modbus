//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using Args;

//////////////////////////////////////////////////////////////////////

namespace KP184
{
    class Actions: Args.Handler
    {
        //////////////////////////////////////////////////////////////////////

        kp184 device = new kp184();

        //////////////////////////////////////////////////////////////////////

        [Help("Set the com port")]
        void port(string com_port)
        {
            Log.Verbose($"COM Port: {com_port}");
            device.port.PortName = com_port;
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the baud rate")]
        void baud(int baud_rate)
        {
            Log.Verbose($"BAUD rate: {baud_rate}");
            device.port.BaudRate = baud_rate;
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the device address")]
        void address(byte device_address)
        {
            if(device_address == 0)
            {
                throw new Args.Error($"Device address can't be 0");
            }
            Log.Verbose($"Device address: {device_address}");
            device.address = device_address;
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Switch the load on or off")]
        void @switch(kp184.load_switch on_or_off)
        {
            Log.Verbose($"Turning load switch {on_or_off}");
            device.set_load_switch(on_or_off);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Switch the load on")]
        void on()
        {
            Log.Verbose($"Turning load switch {kp184.load_switch.on}");
            device.set_load_switch(kp184.load_switch.on);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Switch the load off")]
        void off()
        {
            Log.Verbose($"Turning load switch {kp184.load_switch.off}");
            device.set_load_switch(kp184.load_switch.off);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the load mode")]
        void mode(kp184.load_mode load_mode)
        {
            Log.Verbose($"Setting load mode to {load_mode}");
            device.set_mode(load_mode);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Do a ramp")]
        void ramp(int from, int to, int step, int interval_ms)
        {
            Log.Info($"Ramp: from {from} to {to} in steps of {step} at intervals of {interval_ms}");
            if(Math.Sign(to - from) != Math.Sign(step))
            {
                step = -step;
                Log.Warning($"Changing step from {-step} to {step} so it works");
            }
            int current = from;
            var stop_watch = new System.Diagnostics.Stopwatch();
            var step_time = TimeSpan.FromMilliseconds(step);
            while((step < 0 && current >= to) || (step > 0 && current <= to))
            {
                stop_watch.Restart();
                device.set_current((uint)current);
                current += step;
                stop_watch.Stop();
                int ms_remaining = (step_time - stop_watch.Elapsed).Milliseconds;
                if(ms_remaining > 0)
                {
                    Thread.Sleep(ms_remaining);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set verbose mode")]
        void verbose()
        {
            Log.level = Log.Level.Verbose;
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the log level")]
        void loglevel(Log.Level level)
        {
            Log.level = level;
        }

        //////////////////////////////////////////////////////////////////////

        [Default]
        [Help("Show this help text")]
        public void help()
        {
            show_help("KP184 Controller", "Control the KP184, specify port, baud, address before other commands");
            throw new Args.SilentError();
        }
    }

    //////////////////////////////////////////////////////////////////////

    class Program
    {
        static void Main(string[] args)
        {
            Actions a = new Actions();
            try
            {
                if(!a.execute(args))
                {
                    a.help();
                }
            }
            catch(Args.SilentError e)
            {
            }
            catch(Args.FatalError e)
            {
                Log.Error($"{e.Message}");
            }
            catch(Exception e)
            {
                Log.Error($"{e.Message}");
            }
        }
    }
}
