using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        static COM.COM server;
        static COM.COM monitoring;
        static Queue<string> logs = new Queue<string>();
        static void Main(string[] args)
        {
            Console.Title = "Server";
            Console.OutputEncoding = Encoding.UTF8;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            string port = "COM4";
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

            string portMonitoring=null;
            int speedMonitoring=0;
            if (args.Length >= 4)
            {
                portMonitoring = args[2];
                try
                {
                    speedMonitoring = Convert.ToInt32(args[3]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid speed!");
                    Environment.Exit(1);
                }
            }

            server = new COM.COM(port, speed);

            logs.Enqueue(string.Format("[{2}] {0} - {1}\n", port, speed, DateTime.Now.ToString("dd.MM/yyyy HH:mm:ss")));

            try
            {
                server.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            server.DataReceived += new SerialDataReceivedEventHandler(ByteReceivedFromClient);

            if (portMonitoring != null)
            {
                monitoring = new COM.COM(portMonitoring, speedMonitoring);
                try
                {
                    monitoring.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Environment.Exit(1);
                }
                monitoring.DataReceived += new SerialDataReceivedEventHandler(ByteReceivedFromMonitoring);
            }

            Thread.Sleep(Timeout.Infinite);
        }
        static void ByteReceivedFromClient(object sender, SerialDataReceivedEventArgs e)
        {
            byte b = (byte)((SerialPort)sender).ReadByte();
            char c = Encoding.Default.GetString(new byte[] { b }).First();
            logs.Enqueue(string.Format("[{1}] Byte received: '{0}'.\n", c, DateTime.Now.ToString("dd.MM/yyyy HH:mm:ss")));

            Console.Write(c);
        }
        static void ByteReceivedFromMonitoring(object sender, SerialDataReceivedEventArgs e)
        {
            while(logs.Count!=0)
            {
                byte[] msgbs = Encoding.Default.GetBytes(logs.Dequeue());
                foreach (byte msgs in msgbs)
                    monitoring.SendByte(msgs);
            }
        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (server.IsOpen)
                server.Close();
            if(monitoring!=null)
                if (monitoring.IsOpen)
                    monitoring.Close();
        }
    }
}
