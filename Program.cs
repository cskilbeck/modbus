//////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using Args;

// Usage
//
//  Commands are separated by semicolons
//      port [comport]
//      address [int]
//      baud [baudrate]
//      load [on|off]
//      mode [cc|cv|cw|cr]
//      ramp start end step interval
//      verbose [1..4]

// eg,  to put the load on COM5 at address 1 with baudrate 115200 in constant
//      current mode, then switch on the load, then step from 0mA to 4000mA
//      in steps of 100mA at 100ms intervals, then step from 4000mA down to
//      0mA in steps of 100mA at 100ms intervals, then switch the load off:

// verbose;address 0; loglevel debug; port COM5;baud 115200; mode cc; load; ramp 0 4000 100 100; ramp 4000 0 100 100; load off;help;eff 1.5

//////////////////////////////////////////////////////////////////////

namespace modbus
{
    class Program
    {
        public static string com_port = null;
        public static int baud_rate = 0;
        public static byte address = 0;
        public static kp184.load_switch? load_switch = null;
        public static kp184.load_mode? load_mode = null;
        static kp184 device = new kp184();

        static bool start_device()
        {
            if(com_port == null)
            {
                throw new Args.FatalError("Need a COM port for the device");
            }
            if(baud_rate == 0)
            {
                throw new Args.FatalError("Need a BAUD rate for the device");
            }
            if(address == 0)
            {
                throw new Args.FatalError("Device address not specified");
            }
            if(!device.port.IsOpen)
            {
                device.address = address;
                return device.open(com_port, baud_rate);
            }
            return true;
        }

        public class Arguments: Args.CommandParser
        {
            [Help("Show this help text")]
            void help()
            {
                show_help();
                throw new Args.Error("");
            }

            [Help("Set the com port")]
            void port(string com_port)
            {
                Program.com_port = com_port;
                Log.Verbose($"COM port: {com_port}");
            }

            [Help("Set verbose mode")]
            void verbose()
            {
                Log.level = Log.Level.Verbose;
                Log.Verbose($"Verbose = {Log.level}");
            }

            [Help("Set the log level")]
            void loglevel(Log.Level level)
            {
                Log.level = level;
                Log.Verbose($"Log level: {level}");
            }

            [Help("Set the baud rate")]
            void baud(int baud_rate)
            {
                Program.baud_rate = baud_rate;
                Log.Verbose($"BAUD rate: {baud_rate}");
            }

            [Help("Set the device address")]
            void address(byte device_address)
            {
                if(device_address == 0)
                {
                    throw new Args.Error($"Device address can't be 0");
                }
                Program.address = device_address;
                Log.Verbose($"Address: {device_address}");
            }

            [Help("Switch the load on or off")]
            void @switch(kp184.load_switch on_or_off)
            {
                if(start_device())
                {
                    device.set_load_switch(on_or_off);
                }
            }

            [Help("Switch the load on")]
            void on()
            {
                if(start_device())
                {
                    device.set_load_switch(kp184.load_switch.on);
                }
            }

            [Help("Switch the load off")]
            void off()
            {
                if(start_device())
                {
                    device.set_load_switch(kp184.load_switch.off);
                }
            }

            [Help("Set the load mode")]
            void mode(kp184.load_mode load_mode)
            {
                if(start_device())
                {
                    device.set_mode(load_mode);
                }
            }

            [Help("Do a ramp")]
            void ramp(int from, int to, int step, int interval_ms)
            {
                if(start_device())
                {
                    Log.Info($"Ramp: from {from} to {to} in steps of {step} at intervals of {interval_ms}");
                    if((from < to && step < 0) || (from > to && step > 0))
                    {
                        step = -step;
                        Log.Warning($"Changing step to {step} from {-step} so it works");
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
        }

        static void Main(string[] args)
        {
            try
            {
                new Arguments().execute(args);
            }
            catch(Args.FatalError e)
            {
                Args.Print.Error($"{e.Message}");
            }
        }
    }
}
