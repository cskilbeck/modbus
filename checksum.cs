//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//////////////////////////////////////////////////////////////////////

namespace KP184
{
    //////////////////////////////////////////////////////////////////////

    public class ChecksumException: ApplicationException
    {
        public ChecksumException(string reason) : base(reason)
        {

        }
    }

    public static class checksum
    {
        //////////////////////////////////////////////////////////////////////

        public static void dump_array(string header, byte[] message, int length)
        {
            Console.Error.Write(header);
            string sep = "";
            for(int i = 0; i < length; ++i)
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
            for(int i = 0; i < length; ++i)
            {
                crc ^= message[i];
                for(int j = 0; j < 8; ++j)
                {
                    ushort xor = 0;
                    if((crc & 1) != 0)
                    {
                        xor = 0xa001;
                    }
                    crc = (ushort)((crc >> 1) ^ xor);
                }
            }
            // Flip upper and lower CRC bytes for Kunkin FW Date > 2020
            crc = (ushort)((crc & 0xFFU) << 8 | (crc & 0xFF00U) >> 8);
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

        public static void verify(byte[] message, int length, bool checksum_check)
        {
            ushort crc_got = checksum.extract(message, length);
            ushort crc_computed = checksum.compute(message, length - 2);
            if(crc_computed != crc_got)
            {
                string err = $"Checksum error, got 0x{crc_got:X4}. expected 0x{crc_computed:X4}";
                if(checksum_check)
                {
                    throw new ChecksumException(err);
                }
                else
                {
                    Log.Error(err);
                }
                dump_array("Message: ", message, length);
            }
            else
            {
                Log.Debug($"Checksum OK, got 0x{crc_got:X4}");
            }
        }
    }
}
