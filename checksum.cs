using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbus
{
    public static class checksum
    {
        //////////////////////////////////////////////////////////////////////

        public static ushort get(byte[] message, int length)
        {
            ushort crc = 0xffff;
            for (int i = 0; i < length; ++i)
            {
                crc ^= message[i];
                for (int j = 0; j < 8; ++j)
                {
                    ushort xor = 0;
                    if ((crc & 1) != 0)
                    {
                        xor = 0xa001;
                    }
                    crc = (ushort)((crc >> 1) ^ xor);
                }
            }
            return crc;
        }

        //////////////////////////////////////////////////////////////////////

        public static void set(byte[] message)
        {
            int l = message.Length;
            ushort crc = get(message, l - 2);
            message[l - 1] = (byte)(crc & 0xff);
            message[l - 2] = (byte)(crc >> 8);
        }

    }
}
