using Konsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.ConsoleColor;

namespace Lab4
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        static int PackLength = 3;
        static IConsole client;
        static IConsole server;
        static IConsole monitoring;
        static void Main(string[] args)
        {
            Maximize();

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

            monitoring = Window.OpenBox("Status", 20, 0, 200, 24, new BoxStyle()
            {
                ThickNess = LineThickNess.Single,
                Title = new Colors(White, Blue)
            });

            Channel channel = new Channel();
            Computer mypc = new Computer("mypc", channel, monitoring);
            Computer serv = new Computer("server", channel, server, (new char[] { 'a', 'b', 'c' }).Select(c => (byte)c).ToArray());
            channel.Add(mypc);
            channel.Add(serv);
            serv.StartAndWait();

            while(true)
            {
                string input = "";
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Backspace && input.Length == 0)
                        continue;
                    if (key.Key == ConsoleKey.Backspace && input.Length != 0)
                    {
                        input = input.Remove(input.Length - 1, 1);
                        client.PrintAt(client.CursorLeft - 1, client.CursorTop, ' ');
                        client.CursorLeft--;
                    }
                    else
                    {
                        input += key.KeyChar;
                        Write(client, key.KeyChar.ToString());
                    }
                } while (!(key.Key == ConsoleKey.Enter));
                input = input.Remove(input.Length - 1, 1);
                input = input.Replace(" ", "");
                WriteLine(client, "");

                for (int i = 0; i < input.Length; i += PackLength)
                {
                    string inputForSend;
                    if (i + PackLength < input.Length)
                        inputForSend = input.Substring(i, PackLength);
                    else
                    {
                        int pad = (i + PackLength) - input.Length;
                        inputForSend = input.Substring(i, PackLength - pad);
                        //inputForSend += new string(' ', pad);
                    }
                    byte[] data = Encoding.ASCII.GetBytes(inputForSend);
                    byte[] destinationAddress = serv.Address;
                    byte[] sourceAddress = mypc.Address;
                    Frame frameForSend = new Frame(data, destinationAddress, sourceAddress);
                    mypc.Send(frameForSend);
                }
            }

            Console.ReadKey();
        }
        static void WriteLine(IConsole con, string text = "", ConsoleColor color = White)
        {
            Write(con, text, color);
            con.WriteLine("");
        }
        static void Write(IConsole con, string text = "", ConsoleColor color = White)
        {
            if (con.CursorLeft >= con.WindowWidth - 1)
                con.WriteLine("");

            con.Write(color, text);
        }
        private static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3);
        }
    }
}
