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

        public SerialPort port = new SerialPort();

        //////////////////////////////////////////////////////////////////////

        public bool open(string portName, int baudRate, int databits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            close();

            if(!port.IsOpen)
            {
                port.PortName = portName;
                port.BaudRate = baudRate;
                port.DataBits = databits;
                port.Parity = parity;
                port.StopBits = stopBits;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;
                try
                {
                    port.Open();
                    return true;
                }
                catch(System.IO.IOException e)
                {
                    Log.Error($"Error opening {port.PortName} : {e.GetType()} - {e.Message}");
                }
            }
            return false;
        }

        //////////////////////////////////////////////////////////////////////

        public void flush()
        {
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
        }

        //////////////////////////////////////////////////////////////////////

        public void close()
        {
            if(port.IsOpen)
            {
                port.Close();
            }
        }

        //////////////////////////////////////////////////////////////////////

        public bool write(byte[] message, int len)
        {
            try
            {
                port.Write(message, 0, len);
                return true;
            }
            catch(System.IO.IOException e)
            {
                Log.Error($"Error writing to {port.PortName} : {e.GetType()} - {e.Message}");
            }
            catch(InvalidOperationException e)
            {
                Log.Error($"Error writing to {port.PortName} : {e.GetType()} - {e.Message}");
            }
            catch(TimeoutException e)
            {
                Log.Error($"Error writing to {port.PortName} : {e.GetType()} - {e.Message}");
            }
            return false;
        }

        //////////////////////////////////////////////////////////////////////

        public bool read(byte[] buffer, int len)
        {
            try
            {
                port.Read(buffer, 0, len);
                return true;
            }
            catch(System.IO.IOException e)
            {
                Log.Error($"Error reading from {port.PortName} : {e.GetType()} - {e.Message}");
            }
            catch(InvalidOperationException e)
            {
                Log.Error($"Error reading from {port.PortName} : {e.GetType()} - {e.Message}");
            }
            catch(TimeoutException e)
            {
                Log.Error($"Error reading from {port.PortName} : {e.GetType()} - {e.Message}");
            }
            return false;
        }
    }
}
