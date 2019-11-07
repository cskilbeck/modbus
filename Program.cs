//////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.CommandLineUtils;
using System.Threading;

//////////////////////////////////////////////////////////////////////

namespace modbus
{
    //////////////////////////////////////////////////////////////////////

    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "KP184",
                Description = "Control the KP184"
            };

            app.HelpOption("--help");

            CommandOption com_port = app.Option("-p|--com", "Select com port", CommandOptionType.SingleValue);
            var address = app.Option("-a|--address", "Select device address", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!(com_port.HasValue() && address.HasValue()))
                {
                    app.ShowHelp();
                    return 0;
                }
                byte device_address;
                if (!byte.TryParse(address.Value(), out device_address))
                {
                    app.ShowHelp();
                    return 0;
                }
                kp184 device = new kp184();
                if (!device.open(com_port.Value(), 9600))
                {
                    return 0;
                }
                device.address = device_address;
                device.set_mode(kp184.load_mode.constant_current);
                device.set_load_switch(kp184.load_switch.on);
                for (uint i = 0; i < 3000; i += 100)
                {
                    device.set_current(i);
                    Thread.Sleep(1000);
                }
                device.set_load_switch(kp184.load_switch.off);
                device.close();
                return 0;
            });

            app.Execute(args);
        }
    }
}
