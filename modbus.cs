//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

//////////////////////////////////////////////////////////////////////

namespace modbus
{
    //////////////////////////////////////////////////////////////////////

    public enum mode
    {
        constant_current = 0,
        constant_voltage = 1,
        constant_resistance = 2,
        constant_watts = 3
    };

    //////////////////////////////////////////////////////////////////////

    public enum load_switch
    {
        on,
        off
    };

    //////////////////////////////////////////////////////////////////////

    public struct kp184
    {
        public load_switch switch_status;
        public mode current_mode;
        public uint voltage;
        public uint current;
    };

    //////////////////////////////////////////////////////////////////////

    public class modbus : serial_port
    {
        public byte address;

        //////////////////////////////////////////////////////////////////////
        // assumes the message body is already set up

        private void init_message(byte type, ushort start, ushort registers, ref byte[] message)
        {
            message[0] = address;
            message[1] = type;
            message[2] = (byte)(start >> 8);
            message[3] = (byte)start;
            message[4] = (byte)(registers >> 8);
            message[5] = (byte)registers;
            checksum.set(message, message.Length);
        }

        //////////////////////////////////////////////////////////////////////

        private bool verify_response(int length)
        {
            try
            {
                byte[] response = new byte[length];
                sp.Read(response, 0, length);
                return checksum.verify(response, length);
            }
            catch (InvalidOperationException)
            {
            }
            catch (System.IO.IOException)
            {
            }
            catch (TimeoutException)
            {
            }
            return false;
        }

        //////////////////////////////////////////////////////////////////////

        public bool write_multiple(ushort start, ushort num_registers, short[] values)
        {
            byte[] message = new byte[9 + 2 * num_registers];
            message[6] = (byte)(num_registers * 2);
            for (int i = 0; i < num_registers; i++)
            {
                message[7 + 2 * i] = (byte)(values[i] >> 8);
                message[8 + 2 * i] = (byte)values[i];
            }
            init_message(16, start, num_registers, ref message);
            flush();
            write(message);
            return verify_response(8);
        }

        //////////////////////////////////////////////////////////////////////

        public bool write_register(ushort register, uint value)
        {
            byte[] message = new byte[9 + 2];
            message[7] = (byte)(value >> 8);
            message[8] = (byte)(value & 0xff);
            init_message(6, register, 1, ref message);
            flush();
            write(message);
            return verify_response(13);
        }

        //////////////////////////////////////////////////////////////////////

        public bool get_registers(byte address, ref kp184 status)
        {
            byte[] message = new byte[8];
            byte[] response = new byte[23];
            init_message(3, 0x300, 0, ref message);    // wacky special 0x300 means get them all and the format of the return message is... special
            flush();
            write(message);
            if (!verify_response(response.Length))
            {
                return false;
            }
            status.switch_status = ((response[3] & 1) != 0) ? load_switch.on : load_switch.off;
            status.current_mode = (mode)((response[3] >> 1) & 3);
            status.voltage = ((uint)response[5] << 16) | ((uint)response[6] << 8) | response[7];
            status.current = ((uint)response[8] << 16) | ((uint)response[9] << 8) | response[10];
            return true;
        }
    }
}
