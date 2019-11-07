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

    public class serial_port
    {
        //////////////////////////////////////////////////////////////////////

        public SerialPort sp = new SerialPort();

        //////////////////////////////////////////////////////////////////////

        public bool open(string portName, int baudRate, int databits, Parity parity, StopBits stopBits)
        {
            close();

            if (!sp.IsOpen)
            {
                sp.PortName = portName;
                sp.BaudRate = baudRate;
                sp.DataBits = databits;
                sp.Parity = parity;
                sp.StopBits = stopBits;
                sp.ReadTimeout = 1000;
                sp.WriteTimeout = 1000;
                try
                {
                    sp.Open();
                    return true;
                }
                catch (System.IO.IOException)
                {
                }
            }
            return false;
        }

        //////////////////////////////////////////////////////////////////////

        public void flush()
        {
            sp.DiscardInBuffer();
            sp.DiscardOutBuffer();
        }

        //////////////////////////////////////////////////////////////////////

        public void close()
        {
            if (sp.IsOpen)
            {
                sp.Close();
            }
        }

        //////////////////////////////////////////////////////////////////////

        public bool write(byte[] message)
        {
            try
            {
                sp.Write(message, 0, message.Length);
                return true;
            }
            catch (InvalidOperationException)
            {
            }
            catch (TimeoutException)
            {
            }
            return false;
        }

        //////////////////////////////////////////////////////////////////////

        public bool read(byte[] buffer, int len)
        {
            try
            {
                sp.Read(buffer, 0, len);
                return true;
            }
            catch (InvalidOperationException)
            {
            }
            catch (TimeoutException)
            {
            }
            return false;
        }
    }
}
