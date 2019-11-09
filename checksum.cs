﻿//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//////////////////////////////////////////////////////////////////////

namespace modbus
{
    //////////////////////////////////////////////////////////////////////

    public static class checksum
    {
        //////////////////////////////////////////////////////////////////////

        public static void dump_array(string header, byte[] message, int length)
        {
            Console.Error.Write(header);
            string sep = "";
            for (int i = 0; i < length; ++i)
            {
                Console.Error.Write($"{sep}{message[i]:X2}");
                sep = ",";
            }
            Console.Error.WriteLine();
        }

        //////////////////////////////////////////////////////////////////////
        // get the crc of a message

        public static ushort compute(byte[] message, int length)
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
        // insert a ushort into the last 2 bytes

        public static void insert(byte[] message, int length, ushort crc)
        {
            message[length - 1] = (byte)(crc & 0xff);
            message[length - 2] = (byte)(crc >> 8);
        }

        //////////////////////////////////////////////////////////////////////
        // get the last 2 bytes as a ushort

        public static ushort extract(byte[] message, int length)
        {
            return (ushort)((message[length - 1] & 0xff) | (message[length - 2] << 8));
        }

        //////////////////////////////////////////////////////////////////////
        // overwrite the last 2 bytes with the crc

        public static void set(byte[] message, int length)
        {
            checksum.insert(message, length, checksum.compute(message, length - 2));
        }

        //////////////////////////////////////////////////////////////////////
        // compare existing crc with computed crc

        public static bool verify(byte[] message, int length)
        {
            ushort crc_got = checksum.extract(message, length);
            ushort crc_computed = checksum.compute(message, length);
            if(crc_computed != crc_got)
            {
                Console.Error.WriteLine($"Checksum error, got 0x{crc_got:X4}. expected 0x{crc_computed:X4}");
                dump_array("Message: ", message, length);
                return false;
            }
            return true;
        }

    }
}
