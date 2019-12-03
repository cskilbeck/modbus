//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

//////////////////////////////////////////////////////////////////////

namespace KP184
{
    //////////////////////////////////////////////////////////////////////

    public class serial_port
    {
        //////////////////////////////////////////////////////////////////////

        public SerialPort port = new SerialPort();

        //////////////////////////////////////////////////////////////////////

        public bool open()
        {
            if(!port.IsOpen)
            {
                bool open_it = true;
                if(string.IsNullOrEmpty(port.PortName))
                {
                    Log.Error($"No com port set");
                    open_it = false;
                }
                if(port.BaudRate == 0)
                {
                    Log.Error($"Baud rate not set");
                    open_it = false;
                }
                if(!open_it)
                {
                    return false;
                }
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;
                try
                {
                    Log.Verbose($"Opening {port.PortName} at {port.BaudRate}");
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
            Log.Debug("Flush com port");
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
        }

        //////////////////////////////////////////////////////////////////////

        public void close()
        {
            if(port.IsOpen)
            {
                Log.Debug($"Closing {port.PortName}");
                port.Close();
            }
        }

        //////////////////////////////////////////////////////////////////////

        public bool write(byte[] message, int len)
        {
            if(!open())
            {
                return false;
            }
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
            if(!open())
            {
                return false;
            }
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
