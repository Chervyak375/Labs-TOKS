using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konsole;
using Lab3;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.ConsoleColor;
using System.IO;

namespace Lab3_Refactor
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        static int PackLength = 3;
        static int DetectionCount = 1;
        static int FixingCount = 1;
        static bool RandomErrorEnabled = false;
        static string log = "log.txt";
        static IConsole client;
        static IConsole server;
        static IConsole monitoring;
        static void Main(string[] args)
        {
            if (File.Exists(log))
                File.Delete(log);

            while (true)
            {
                Console.WriteLine("Enable random error? (y/n)");
                char key = Console.ReadKey().KeyChar;
                if(!"yn".Contains(key))
                {
                    Console.Clear();
                    continue;
                }
                RandomErrorEnabled = key == 'y' ? true : false;
                Console.Clear();
                break;
            }

            Maximize();

            client = Window.OpenBox("Input", 20, 30, new BoxStyle()
            {
                ThickNess = LineThickNess.Single,
                Title = new Colors(White, Blue)
            });

            server = Window.OpenBox("Output", 20, 30, new BoxStyle()
            {
                ThickNess = LineThickNess.Single,
                Title = new Colors(White, Blue)
            });

            monitoring = Window.OpenBox("Status", 20, 0, 80, 60, new BoxStyle()
            {
                ThickNess = LineThickNess.Single,
                Title = new Colors(White, Blue)
            });

            WriteLine(monitoring, "CRC Settings:");
            LogLine("CRC Settings:");
            WriteLine(monitoring, string.Format("  Input Length = {0}", PackLength));
            LogLine(string.Format("  Input Length = {0}", PackLength));
            WriteLine(monitoring, string.Format("  Detection count = {0}", DetectionCount));
            LogLine(string.Format("  Detection count = {0}", DetectionCount));
            WriteLine(monitoring, string.Format("  Fixing count = {0}", FixingCount));
            LogLine(string.Format("  Fixing count = {0}", FixingCount));
            WriteLine(monitoring, "");
            LogLine("");

            while (true)
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
                WriteLine(client, "");

                for (int i = 0; i < input.Length; i += PackLength)
                {
                    string _input;
                    if (i + PackLength < input.Length)
                        _input = input.Substring(i, PackLength);
                    else
                    {
                        int pad = (i + PackLength) - input.Length;
                        _input = input.Substring(i, PackLength - pad);
                        _input += new string(' ', pad);
                    }
                    CRC crcInput = new CRC();
                    string poly = crcInput.GetBasePoly(_input.Length * 8);

                    monitoring.Write("Input = ");
                    Log("Input = ");
                    monitoring.Write(Green, _input);
                    Log(_input);
                    monitoring.Write(" Poly = ");
                    Log(" Poly = ");
                    monitoring.Write(Yellow, poly);
                    Log(poly);
                    monitoring.WriteLine("");
                    LogLine("");

                    BitArray dataTransfer = new BitArray(crcInput.Encode(Encoding.ASCII.GetBytes(_input)));

                    if (RandomErrorEnabled)
                    {
                        BitArray dataTransferOld = (BitArray)dataTransfer.Clone();
                        dataTransfer = CRC.RandomError(dataTransferOld);
                        monitoring.WriteLine("Send to Channel = {0} and after random error = {1}",
                            CRC.ConvertBitArrayToString(dataTransferOld, dataTransferOld.Length),
                            CRC.ConvertBitArrayToString(dataTransfer, dataTransfer.Length));
                        LogLine(string.Format("Send to Channel = {0} and after random error = {1}",
                            CRC.ConvertBitArrayToString(dataTransferOld, dataTransferOld.Length),
                            CRC.ConvertBitArrayToString(dataTransfer, dataTransfer.Length)));
                    }

                    monitoring.Write("CRC = ");
                    Log("CRC = ");
                    monitoring.Write(Blue, CRC.ConvertBitArrayToString(dataTransfer, dataTransfer.Length));
                    Log(CRC.ConvertBitArrayToString(dataTransfer, dataTransfer.Length));
                    monitoring.WriteLine("");
                    LogLine("");

                    CRC crcOutput = new CRC();
                    //string output = crcOutput.Decode(CRC.ConvertBitArrayToString(dataTransfer, dataTransfer.Length), input.Length);
                    string output = Encoding.ASCII.GetString(crcOutput.Decode(CRC.BitArrayToByteArray(dataTransfer), _input.Length * 8));

                    monitoring.Write("Output = ");
                    Log("Output = ");
                    monitoring.Write(DarkGreen, output);
                    Log(output);
                    monitoring.WriteLine("");
                    LogLine("");

                    server.Write(output);
                    monitoring.WriteLine("");
                    LogLine("");

                    monitoring.WriteLine(" ---- Encoding Steps ----");
                    LogLine(" ---- Encoding Steps ----");
                    foreach (string l in crcInput.Log)
                    {
                        monitoring.WriteLine(l);
                        LogLine(l);
                    }

                    monitoring.WriteLine(" ---- Decoding Steps ----");
                    LogLine(" ---- Decoding Steps ----");
                    foreach (string l in crcOutput.Log)
                    {
                        monitoring.WriteLine(l);
                        LogLine(l);
                    }
                    monitoring.WriteLine(" ---- ---- ----");
                    LogLine(" ---- ---- ----");

                    monitoring.WriteLine("");
                    LogLine("");
                }
                server.WriteLine("");
            }
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
        private static void Log(string messsage)
        {
            File.AppendAllText(log, messsage);
        }
        private static void LogLine(string messsage)
        {
            Log(messsage);
            Log("\n");
        }
    }
}
