using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konsole;
using static System.ConsoleColor;

namespace lab2
{
    class ByteStuffing
    {
        static IConsole client;
        static IConsole server;
        static IConsole monitoring;
        static void Main(string[] args)
        {
            char flag = Convert.ToChar(100 + 3);
            char esc = '+';

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

            while (true)
            {
                string input = "";
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Backspace && input.Length!=0)
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
                } while (key.Key != ConsoleKey.Enter);
                input = input.Remove(input.Length-1, 1);
                WriteLine(client, "");

                Pack pack = Pack.Encode(input, flag);

                Write(monitoring, "Flag = '");
                Write(monitoring, flag.ToString(), ConsoleColor.DarkRed);
                Write(monitoring, "' (");
                Write(monitoring, string.Format("{0}h", (byte)flag), ConsoleColor.DarkRed);
                Write(monitoring, ") ");

                Write(monitoring, "Esc = '");
                Write(monitoring, esc.ToString(), ConsoleColor.Red);
                Write(monitoring, "'");
                WriteLine(monitoring);

                Write(monitoring, "Message = '");
                if (pack.Str.Length != 0)
                {
                    Write(monitoring, pack.Str.First().ToString(), DarkGreen);
                    Write(monitoring, pack.Str.Substring(1, pack.NumByteCout), Green);
                    int index = 1 + pack.NumByteCout;
                    foreach (int strPossReplaced in pack.StrPossReplaced)
                    {
                        int l = strPossReplaced - index;
                        string strEncPart = pack.Str.Substring(index, l);
                        Write(monitoring, strEncPart);
                        Write(monitoring, pack.Str[strPossReplaced].ToString(), Red);
                        index += l;
                        index += 1;
                        Write(monitoring, pack.Str.Substring(index, pack.NumByteCout), Green);
                        index += pack.NumByteCout;
                    }

                    Write(monitoring, pack.Str.Substring(index, pack.Str.Length - index));
                }
                WriteLine(monitoring, "'");

                char flagDetected = pack.Flag;
                string output = Pack.Decode(pack.Str, flagDetected);
                WriteLine(server, output);
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
    }
}
