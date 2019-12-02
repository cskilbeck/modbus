//////////////////////////////////////////////////////////////////////

using System;
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

        [Default]
        [Help("Show this help text")]
        void help()
        {
            string help_string = "Separate commands with semicolon, EG:\n\n" +
                                 "kp184 port com5; baud 115200; address 1; switch on";
            show_help("KP184 Controller", help_string);
            throw new Args.FatalError("");
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the com port")]
        void port(string com_port)
        {
            device.port.PortName = com_port;
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

        [Help("Set the baud rate")]
        void baud(int baud_rate)
        {
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
            device.address = device_address;
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Switch the load on or off")]
        void @switch(kp184.load_switch on_or_off)
        {
            device.set_load_switch(on_or_off);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Switch the load on")]
        void on()
        {
            device.set_load_switch(kp184.load_switch.on);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Switch the load off")]
        void off()
        {
            device.set_load_switch(kp184.load_switch.off);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the load mode")]
        void mode(kp184.load_mode load_mode)
        {
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
                Log.Info($"Changing step to {step} from {-step} so it works");
            }
            Log.Info($"Stepping from {from} to {to} in steps of {step}mA at intervals of {interval_ms}ms");
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
    }

    //////////////////////////////////////////////////////////////////////

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Actions().execute(args);
            }
            catch(Args.FatalError e)
            {
                Args.Print.Error($"{e.Message}");
            }
        }
    }
}
