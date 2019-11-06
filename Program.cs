using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace modbus
{
    class Program
    {
        static void checksum(byte[] message)
        {
            int l = message.Length;
            if(l < 3)
            {
                throw new ArgumentException($"message is only {l} long, too short to be checksummed (must be >= 3)");
            }
            ushort crc = 0xffff;
            for (int i=0; i<l-2; ++i)
            {
                crc ^= message[i];
                for(int j=0; j<8; ++j)
                {
                    ushort xor = 0;
                    if((crc & 1) != 0)
                    {
                        xor = 0xa001;
                    }
                    crc = (ushort)((crc >> 1) ^ xor);
                }
            }
            message[l - 1] = (byte)(crc & 0xff);
            message[l - 2] = (byte)(crc >> 8);
        }

        static void Main(string[] args)
        {
            //byte[] message = { 0x01, 0x06, 0x01, 0x16, 0x00, 0x01, 0x04, 0x00, 0x00, 0x07, 0xD0, 0x0C, 0x9D };
            byte[] message = { 0x01, 0x06, 0x01, 0x16, 0x00, 0x01, 0x04, 0x00, 0x00, 0x07, 0xD0, 0x00, 0x00 };
            checksum(message);
        }
    }
}
