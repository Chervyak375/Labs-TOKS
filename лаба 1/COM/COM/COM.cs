using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace COM
{
    public class COM : SerialPort
    {
        public COM(string portName, int baudRate) : base(portName, baudRate) { }
        public void SendData(byte[] data)
        {
            RtsEnable = true;
            Write(data, 0, data.Length);
            Thread.Sleep(100);
            RtsEnable = false;
        }
        public void SendByte(byte b)
        {
            SendData(new byte[] { b });
        }
    }
}
