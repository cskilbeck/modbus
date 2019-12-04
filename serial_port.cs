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

        public void open()
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
                if(open_it)
                {
                    port.DataBits = 8;
                    port.Parity = Parity.None;
                    port.StopBits = StopBits.One;
                    port.ReadTimeout = 1000;
                    port.WriteTimeout = 1000;
                    try
                    {
                        Log.Verbose($"Opening {port.PortName} at {port.BaudRate}");
                        port.Open();
                    }
                    catch(System.IO.IOException e)
                    {
                        Log.Error($"Error opening {port.PortName}");
                        throw;
                    }
                }
            }
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

        public void write(byte[] message, int len)
        {
            open();
            try
            {
                port.Write(message, 0, len);
            }
            catch(System.IO.IOException e)
            {
                Log.Error($"Error writing to {port.PortName}");
                throw;
            }
            catch(InvalidOperationException e)
            {
                Log.Error($"Error writing to {port.PortName}");
                throw;
            }
            catch(TimeoutException e)
            {
                Log.Error($"Timeout reading from {port.PortName}");
            }
        }

        //////////////////////////////////////////////////////////////////////

        public void read(byte[] buffer, int len)
        {
            open();
            try
            {
                port.Read(buffer, 0, len);
            }
            catch(System.IO.IOException e)
            {
                Log.Error($"Error reading from {port.PortName}");
                throw;
            }
            catch(InvalidOperationException e)
            {
                Log.Error($"Error reading from {port.PortName}");
                throw;
            }
            catch(TimeoutException e)
            {
                Log.Error($"Timeout reading from {port.PortName}");
            }
        }
    }
}
