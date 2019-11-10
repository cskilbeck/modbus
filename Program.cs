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

            CommandOption com_port = app.Option("-p|--com", "com port (string)", CommandOptionType.SingleValue);
            CommandOption address = app.Option("-a|--address", "device address (byte)", CommandOptionType.SingleValue);
            CommandOption baud_rate = app.Option("-b|--baud", "baud rate", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!(com_port.HasValue() && address.HasValue() && baud_rate.HasValue()))
                {
                    app.ShowHelp();
                    return 0;
                }
                byte device_address;
                if (!byte.TryParse(address.Value(), out device_address))
                {
                    Console.Error.WriteLine($"Invalid device address: {address.Value()}");
                    app.ShowHelp();
                    return 0;
                }
                int actual_baud_rate;
                if(!int.TryParse(baud_rate.Value(), out actual_baud_rate))
                {
                    Console.Error.WriteLine($"Invalid baud rate: {baud_rate.Value()}");
                    app.ShowHelp();
                    return 0;
                }
                kp184 device = new kp184();
                if (!device.open(com_port.Value(), actual_baud_rate))
                {
                    return 0;
                }
                Console.WriteLine($"COM Port: {com_port.Value()}");
                Console.WriteLine($"BAUD Rate: {actual_baud_rate}");
                Console.WriteLine($"Address: {device_address}");
                device.address = device_address;
                device.get_status();
                //device.set_mode(kp184.load_mode.constant_current);
                //device.set_load_switch(kp184.load_switch.on);
                //for (uint i = 0; i < 3000; i += 100)
                //{
                //    device.set_current(i);
                //    Thread.Sleep(1000);
                //}
                //device.set_load_switch(kp184.load_switch.off);
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
