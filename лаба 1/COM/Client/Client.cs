using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        static COM.COM client;
        static void Main(string[] args)
        {
            Console.Title = "Client";
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            string port = "COM3";
            int speed = 96000;

            if (args.Length >= 2)
            {
                port = args[0];
                try
                {
                    speed = Convert.ToInt32(args[1]);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Invalid speed!");
                    Environment.Exit(1);
                }
            }

            client = new COM.COM(port, speed);
            try
            {
                client.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                byte b = Encoding.Default.GetBytes(key.KeyChar.ToString()).First();
                client.SendByte(b);
            }
        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if(client.IsOpen)
                client.Close();
        }
    }
}
