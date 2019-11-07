//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

//////////////////////////////////////////////////////////////////////

namespace modbus
{
    //////////////////////////////////////////////////////////////////////

    class Program
    {
        //////////////////////////////////////////////////////////////////////

        static bool verify_checksum(byte[] message)
        {
            return checksum.verify(message, message.Length);
        }

        //////////////////////////////////////////////////////////////////////

        static void set_load_switch(load_switch s)
        {
            byte[] message = { 0x01, 0x06, 0x01, 0x0E, 0x00, 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            if (s == load_switch.on)
            {
                message[10] = 1;
            }
        }

        //////////////////////////////////////////////////////////////////////

        static void set_mode(mode m)
        {
            int mode = (int)m;
            byte[] message = { 0x01, 0x06, 0x01, 0x10, 0x00, 0x01, 0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 };
            message[7] = (byte)(mode >> 24);
            message[8] = (byte)(mode >> 16);
            message[9] = (byte)(mode >> 8);
            message[10] = (byte)(mode >> 0);
        }

        //////////////////////////////////////////////////////////////////////

        static bool get_status(out int volts, out int amps)
        {
            byte[] message = { 0x01, 0x03, 0x03, 0x00, 0x00, 0x00, 0x8E, 0x45 };

            volts = 0;
            amps = 0;
            return true;
        }

        //////////////////////////////////////////////////////////////////////

        static void Main(string[] args)
        {
            byte[] message = { 0x01, 0x06, 0x01, 0x16, 0x00, 0x01, 0x04, 0x00, 0x00, 0x07, 0xD0, 0x00, 0x00 };
            checksum.set(message, message.Length);
            bool x = verify_checksum(message);
        }
    }
}
