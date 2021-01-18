using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4
{
    class Pack
    {
        public string Mac
        { 
            get
            {
                return pack.Substring(0, 10);
            }
        }
        public string Message
        {
            get
            {
                return pack.Substring(11, pack.Length - 11);
            }
        }
        public int Length
        {
            get
            {
                return pack.Length;
            }
        }
        private string pack = "";
        public Pack(string mac, string message)
        {
            pack += mac + message;
        }
        public char GetData(int i)
        {
            return pack[i];
        }
    }
}
