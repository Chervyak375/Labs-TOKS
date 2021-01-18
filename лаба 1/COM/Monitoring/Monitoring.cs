using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monitoring
{
    class Monitoring
    {
        static COM.COM monitoring;
        static void Main(string[] args)
        {
            Console.Title = "Monitoring";
            Console.OutputEncoding = Encoding.UTF8;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            string port = "COM5";
            int speed = 96000;

            if (args.Length >= 2)
            {
                port = args[0];
                try
                {
                    speed = Convert.ToInt32(args[1]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid speed!");
                    Environment.Exit(1);
                }
            }

            monitoring = new COM.COM(port, speed);

            try
            {
                monitoring.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
    monitoring.DataReceived += new SerialDataReceivedEventHandler(ByteReceivedFromServer);

            while (true)
            {
                monitoring.SendByte(11);
                Thread.Sleep(1000);
            }

            Thread.Sleep(Timeout.Infinite);
        }

        static void ByteReceivedFromServer(object sender, SerialDataReceivedEventArgs e)
        {
            byte b = (byte)((SerialPort)sender).ReadByte();
            char c = Encoding.Default.GetString(new byte[] { b }).First();
            Console.Write(c);
        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (monitoring.IsOpen)
                monitoring.Close();
        }
    }
}
