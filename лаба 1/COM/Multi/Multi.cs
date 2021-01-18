using Konsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ConsoleColor;

namespace Multi
{
    class Multi
    {
        static COM.COM clientCOM;
        static COM.COM serverCOM;
        static string initClientLine;
        static string initServerLine;
        static string input = "";
        static List<string> logs = new List<string>();
        static int endMonitoringPos = 0;
        static IConsole client;
        static IConsole server;
        static IConsole monitoring;
        static void Main(string[] args)
        {
            Console.Title = "Multi";

            string portClient = "COM3";
            int speedClient = 9600;
            string portServer = "COM4";
            int speedServer = 9600;

            client = Window.OpenBox("Input", 20, 12, new BoxStyle()
            {
                ThickNess = LineThickNess.Single,
                Title = new Colors(White, Blue)
            });

            server = Window.OpenBox("Output", 20, 12, new BoxStyle()
            {
                ThickNess = LineThickNess.Single,
                Title = new Colors(White, Blue)
            });

            monitoring = Window.OpenBox("Status", 20, 0, 80, 24, new BoxStyle()
            {
                ThickNess = LineThickNess.Single,
                Title = new Colors(White, Blue)
            });

            if (args.Length >= 4)
            {
                bool isClient = true;
                portClient = args[0];
                portServer = args[2];
                try
                {
                    speedClient = Convert.ToInt32(args[1]);
                    isClient = false;
                    speedServer = Convert.ToInt32(args[3]);
                }
                catch (Exception e)
                {
                    if(isClient)
                        Write(client, "Invalid speed!");
                    else
                        Write(server, "Invalid speed!");
                    Console.ReadKey(true);
                    Environment.Exit(1);
                }
            }

            clientCOM = new COM.COM(portClient, speedClient);

            try
            {
                clientCOM.Open();
            }
            catch (Exception e)
            {
                Write(client, e.Message);
                Console.ReadKey(true);
                Environment.Exit(1);
            }

            initClientLine = string.Format("Init {0} : {1} baud rate; stop bits {2}; RTS is enable {3}; Parity {4}.", portClient, speedClient, clientCOM.StopBits, clientCOM.RtsEnable, clientCOM.Parity);
            logs.Add(initClientLine);
            endMonitoringPos++;
            WriteLine(monitoring, initClientLine);

            serverCOM = new COM.COM(portServer, speedServer);

            try
            {
                serverCOM.Open();
            }
            catch (Exception e)
            {
                Write(server, e.Message);
                Console.ReadKey(true);
                Environment.Exit(1);
            }
            
            initServerLine = string.Format("Init {0} - {1} baud rate; stop bits {2}; RTS is enable {3}; Parity {4}.", portServer, speedServer, serverCOM.StopBits, serverCOM.RtsEnable, serverCOM.Parity);
            logs.Add(initServerLine);
            endMonitoringPos++;
            WriteLine(monitoring, initServerLine);

            serverCOM.DataReceived += new SerialDataReceivedEventHandler(ByteReceivedFromClient);

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if(key.Key == ConsoleKey.UpArrow)
                {
                    if (endMonitoringPos < logs.Count)
                        endMonitoringPos++;

                    Logging(endMonitoringPos);
                    continue;
                } else if(key.Key == ConsoleKey.DownArrow)
                {
                    if (endMonitoringPos - 21 > 0)
                        endMonitoringPos--;

                    Logging(endMonitoringPos);
                    continue;
                }

                if(key.Key == ConsoleKey.Enter)
                    WriteLine(client, "");
                else
                    Write(client, key.KeyChar.ToString());
                byte b = Encoding.Default.GetBytes(key.KeyChar.ToString()).First();
                string log = string.Format("Byte sended: '{0}'.\n", (char)b);
                logs.Add(log);
                WriteLine(monitoring, log);
                endMonitoringPos++;
                clientCOM.SendByte(b);
            }
        }
        static void ByteReceivedFromClient(object sender, SerialDataReceivedEventArgs e)
        {
            byte b = (byte)((SerialPort)sender).ReadByte();
            char c = Encoding.Default.GetString(new byte[] { b }).First();
            string log = string.Format("Byte received: '{0}'.\n", c);
            logs.Add(log);
            WriteLine(monitoring, log);
            endMonitoringPos++;
            if (c == '\r')
                WriteLine(server, "");
            else
                Write(server, c.ToString());
        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (clientCOM.IsOpen)
                clientCOM.Close();
            if (serverCOM.IsOpen)
                serverCOM.Close();
        }
        static void WriteLine(IConsole con, string text)
        {
            Write(con, text);
            con.WriteLine("");
        }
        static void Write(IConsole con, string text)
        {
            if (con.CursorLeft >= con.WindowWidth - 1)
                con.WriteLine("");

            con.Write(White, text);
        }
        static void Logging(int end)
        {
            monitoring.Clear();

            int start = end - 21;
            if (start < 0)
                start = 0;
            for (int i = start; i < end; i++)
                WriteLine(monitoring, logs[i]);
        }
    }
}
