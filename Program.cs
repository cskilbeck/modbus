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

        [Help("Do nothing for N milliseconds")]
        void delay(int ms)
        {
            Log.Verbose($"Sleeping for {ms}ms");
            Thread.Sleep(ms);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the load mode")]
        void mode(kp184.load_mode load_mode)
        {
            Log.Verbose($"Setting load mode to {load_mode}");
            device.set_mode(load_mode);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Enable (true) or disable (false) the checksum check")]
        void checksum(bool enable)
        {
            Log.Info($"Checksum checking: {enable}");
            device.checksum_check = enable;
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
            var total_time = new System.Diagnostics.Stopwatch();
            var step_time = TimeSpan.FromMilliseconds(interval_ms);
            total_time.Start();
            while((step < 0 && current >= to) || (step > 0 && current <= to))
            {
                stop_watch.Restart();
                device.set_current((uint)current);
                Log.Info($"{current,9}mA, elapsed: {total_time.Elapsed}");
                current += step;
                stop_watch.Stop();
                double ms_remaining = (step_time - stop_watch.Elapsed).TotalMilliseconds;
                if(ms_remaining > 0)
                {
                    Thread.Sleep((int)ms_remaining);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the current in milliamps")]
        void current(uint current)
        {
            Log.Verbose($"Set current to: {current}");
            device.set_current(current);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the watts in deci Watts (0.01W)")]
        void power(uint watts)
        {
            Log.Verbose($"Set watts to: {watts}");
            device.set_power(watts);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Set the resistance in 0.1 Ohm")]
        void resistance(uint ohms)
        {
            Log.Verbose($"Set ohms to: {ohms}");
            device.set_resistance(ohms);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Returns the status of KP184")]
        void get_status()
        {
            Log.Verbose($"Calling get_status from KP184");
            device.get_status();
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Returns if device is On (=1) or Off (=0)")]
        void get_switch()
        {
            Log.Verbose($"Calling get_OnOff from KP184");
            int OnOff = device.get_load_switch();
            Console.WriteLine(OnOff);
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Returns the mode (cv=0, cc=1, cr=2, cw=3) of the device")]
        void get_mode()
        {
            Log.Verbose($"Calling get_Mode from KP184");
            int Mode = device.get_mode();
            Log.Info($"Mode is {Mode} ({kp184.load_mode_as_string(Mode)})");
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Reads the current in mA")]
        void get_current()
        {
            Log.Verbose($"Calling get_Mode from KP184");
            int current = device.get_current();
            Log.Info($"Current is {current}mA");
        }

        //////////////////////////////////////////////////////////////////////

        [Help("Reads the voltage in mV")]
        void get_voltage()
        {
            Log.Verbose($"Calling get_Mode from KP184");
            int volts = device.get_voltage();
            Log.Info($"Voltage is {volts}mV");
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
#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
