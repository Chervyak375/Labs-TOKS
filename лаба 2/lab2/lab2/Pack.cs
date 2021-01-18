using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2
{
    public class Pack
    {
        public string Str
        {
            get
            {
                return str;
            }
        }
        public char Flag
        {
            get
            {
                return flag;
            }
        }
        public char Esc
        {
            get
            {
                return esc;
            }
        }
        public int NumByteCout
        {
            get
            {
                return numByteCout;
            }
        }
        public int[] StrSourcePossReplaced
        {
            get
            {
                return strSourcePossReplaced;
            }
        }
        public int[] StrPossReplaced
        {
            get
            {
                return strPossReplaced;
            }
        }

        private string str;
        private char flag;
        private char esc;
        private int numByteCout;
        private int[] strSourcePossReplaced;
        private int[] strPossReplaced;

        public Pack() { }
        public Pack(string str, char flag, char esc, int numByteCout=0, int[] strSourcePossReplaced=null, int[] strPossReplaced=null)
        {
            this.str = str;
            this.flag = flag;
            this.esc = esc;
            this.numByteCout = numByteCout;
            this.strSourcePossReplaced = strSourcePossReplaced;
            this.strPossReplaced = strPossReplaced;
        }

        public static Pack Encode(string strSource, char flag, char esc = '+')
        {
            if (strSource.Length == 0)
                return new Pack("", flag, esc);

            List<int> strSourcePossReplaced = new List<int>();
            List<int> strPossReplaced = new List<int>();

            int numByteCout = (int)Math.Ceiling(Math.Log10(strSource.Length));
            if (strSource.Length == 1)
                numByteCout = 1;
            string numByteCoutMask = new string('0', numByteCout) + ".##";
            string str = numByteCout.ToString();

            int i = 0;
            int flagIndex;

            while (true)
            {
                flagIndex = strSource.IndexOf(flag, i);
                if (flagIndex == -1)
                {
                    int indexForReplace = strSource.Length - i;
                    str += indexForReplace.ToString(numByteCoutMask);
                    str += strSource.Substring(i, indexForReplace);
                    break;
                }
                else
                {
                    strSourcePossReplaced.Add(flagIndex);
                    int indexForReplace = flagIndex - i;
                    str += indexForReplace.ToString(numByteCoutMask);
                    string sub = strSource.Substring(i, indexForReplace);
                    str += sub;
                    strPossReplaced.Add(str.Length);
                    str += esc;
                    i += indexForReplace + 1;
                }
            }

            return new Pack(str, flag, esc, numByteCout, strSourcePossReplaced.ToArray(), strPossReplaced.ToArray());
        }

        public static string Decode(string str, char flag)
        {
            if (str.Length == 0)
                return str;

            int l = str.Length;
            int numByteCout = int.Parse(str.First().ToString());
            int indexForReplace = int.Parse(str.Substring(1, numByteCout));
            int i = 1 + numByteCout;
            string result = str.Substring(i, indexForReplace);
            result += flag;

            while (true)
            {
                i += indexForReplace + 1;
                if (i > str.Length)
                    break;
                indexForReplace = int.Parse(str.Substring(i, numByteCout));
                i += numByteCout;
                result += str.Substring(i, indexForReplace);
                result += flag;
            }

            result = result.Remove(result.Length - 1, 1);

            return result;
        }
    }
}
