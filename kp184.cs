//////////////////////////////////////////////////////////////////////
// the wacky KP184 electronic load

using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

//////////////////////////////////////////////////////////////////////

namespace KP184
{
    public class kp184: serial_port
    {
        public byte address;

        public load_switch switch_status;
        public load_mode switch_mode;
        public uint voltage;
        public uint current;

        public bool checksum_check = true;

        //////////////////////////////////////////////////////////////////////

        public enum load_mode
        {
            cv = 0,
            cc = 1,
            cr = 2,
            cw = 3
        };

        //////////////////////////////////////////////////////////////////////

        public enum load_switch
        {
            off = 0,
            on = 1
        };

        //////////////////////////////////////////////////////////////////////
        // these are not normal modbus (0x03 means read the whole bank)

        public enum command
        {
            read_registers = 0x03,
            write_single = 0x06,
            write_multiple = 0x16
        };

        //////////////////////////////////////////////////////////////////////

        public enum register
        {
            load_switch = 0x010e,
            load_mode = 0x0110,
            volts = 0x0112,
            current = 0x0116,
            resistance = 0x011a,
            watts = 0x011e,
            measured_volts = 0x0122,
            measured_amps = 0x0126
        };

        //////////////////////////////////////////////////////////////////////

        private const int reply_delay = 100;

        private static void delay()
        {
            Thread.Sleep(reply_delay);
        }

        //////////////////////////////////////////////////////////////////////
        // assumes the message body is already set up

        private void init_message(command type, ushort start, ushort registers, ref byte[] message)
        {
            message[0] = address;
            message[1] = (byte)type;
            message[2] = (byte)(start >> 8);
            message[3] = (byte)start;
            message[4] = (byte)(registers >> 8);
            message[5] = (byte)registers;
            checksum.set(message, message.Length);
        }

        //////////////////////////////////////////////////////////////////////

        private void send_message(command type, ushort start, ushort registers, ref byte[] message)
        {
            if(address == 0)
            {
                throw new ApplicationException($"Device address has not been set");
            }
            init_message(type, start, registers, ref message);
            write(message, message.Length);
        }

        //////////////////////////////////////////////////////////////////////
        // get a checked modbus response

        private byte[] get_response(int length)
        {
            byte[] response = new byte[length];
            read(response, length);
            if(checksum_check)
            {
                checksum.verify(response, length);
            }
            return response;
        }

        //////////////////////////////////////////////////////////////////////
        // write multiple registers to modbus
        // this is untested and probably doesn't work, there's no example in the manual

        public void write_multiple(register start_register, ushort num_registers, uint[] values)
        {
            byte[] message = new byte[9 + 4 * num_registers];
            message[6] = (byte)(num_registers * 4);
            int rstart = 7;
            int rend = 7 + num_registers * 4;
            for(int i = rstart; i < rend;)
            {
                message[i++] = (byte)(values[i] >> 24);
                message[i++] = (byte)(values[i] >> 16);
                message[i++] = (byte)(values[i] >> 8);
                message[i++] = (byte)(values[i] >> 0);
            }
            send_message(command.write_multiple, (ushort)start_register, num_registers, ref message);
            delay();
            get_response(8);
        }

        //////////////////////////////////////////////////////////////////////
        // write a single modbus register

        public void write_register(register register, uint value)
        {
            byte[] message = new byte[11 + 2];
            message[6] = sizeof(uint); //4
            message[7] = (byte)(value >> 24);
            message[8] = (byte)(value >> 16);
            message[9] = (byte)(value >> 8);
            message[10] = (byte)(value >> 0);
            send_message(command.write_single, (ushort)register, 1, ref message);
            get_response(9);
        }

        //////////////////////////////////////////////////////////////////////
        // get the KP184 status

        public void get_status()
        {
            byte[] message = new byte[8];
            // wacky special read at 0x300 means get them all and the format of the return message is... special
            send_message(command.read_registers, 0x300, 0, ref message);
            delay();
            byte[] response = get_response(23);
            if(response != null)
            {
                switch_status = (load_switch)(response[3] & 1);
                switch_mode = (load_mode)((response[3] >> 1) & 7);  // TODO (chs): then look this up, the values in the status are wacky
                voltage = ((uint)response[5] << 16) | ((uint)response[6] << 8) | response[7];
                current = ((uint)response[8] << 16) | ((uint)response[9] << 8) | response[10];
                Log.Info("Status:");
                Log.Info($"   Switch is {switch_status}");
                Log.Info($"   Mode is {switch_mode}");
                Log.Info($"   Current is {current}");
                Log.Info($"   Voltage is {voltage}");
            }
        }

        //////////////////////////////////////////////////////////////////////
        // helpers

        public void set_current(uint milliamps)
        {
            Log.Verbose($"Set current to {milliamps}mA");
            write_register(register.current, milliamps);
        }

        public void set_mode(load_mode mode)
        {
            Log.Verbose($"Set mode to {mode}");
            write_register(register.load_mode, (uint)mode);
        }

        public void set_load_switch(load_switch on_or_off)
        {
            Log.Verbose($"Set switch {on_or_off}");
            write_register(register.load_switch, (uint)on_or_off);
        }
    }
}
