using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    class Program
    {
        //Number of errors detected
        static int r = 1;
        static int S = r;
        //Number of errors corrected
        static int b = 1;

        //Code distance value
        public static int GetDmin() => r + 1;
        //Number of check digits
        public static int GetK(int m)
        {
            return (int)Math.Ceiling(
                Math.Log(
                        (double)(int)(
                            m + 1 + Math.Ceiling(Math.Log((double)(m + 1), 2))
                        ), 2
                    )
                );
        }
        public static BitArray GetBasePolyByPower(int k)
        {
            return ConvertStringToBitArray(new string[] {
                "11",
                "111",
                "1011",
                "10011",
                "100101"
            }[k - 1]);
        }
        static void Main(string[] args)
        {
            string input = Console.ReadLine();
            BitArray bsInput = ConvertStringToBitArray(input);
            int k = GetK(bsInput.Length);
            BitArray basePoly = GetBasePolyByPower(k);
            BitArray bsSource = BitArrayAdd(bsInput, new BitArray(k));
            BitArray bsRemainder = Div(bsSource, basePoly);
            BitArray bsResult = BitArrayAdd(bsInput, bsRemainder);
            Console.WriteLine("Result: {0}", ConvertBitArrayToString(bsResult, bsResult.Length));
            // ---- Generate Random Error ----
            //BitArray bsChannel = RandomError(bsResult);
            //BitArray bsChannel = ConvertStringToBitArray("01111011");
            BitArray bsChannel = bsResult;
            // ---- ---- ---- ---- ---- ---- ----
            Console.WriteLine("Send by channel: {0}", ConvertBitArrayToString(bsChannel, bsChannel.Length));

            //Recive from Channel Bits
            BitArray bsRecived = bsChannel;
            //basePoly = SubBitArray(bsRecived, bsRecived.Length - k, k);
            BitArray check = Div(bsRecived, basePoly);
            Console.WriteLine("Check: {0}", ConvertBitArrayToString(check, check.Length));
            int w = GetWeight(check);
            int shiftCount = 0;
            while(w > S)
            {
                bsRecived = ShiftLeft(bsRecived);
                Console.WriteLine("[{0}] Shit: {1}", shiftCount + 1, ConvertBitArrayToString(bsRecived, bsRecived.Length));
                check = Div(bsRecived, basePoly);
                Console.WriteLine("[{0}] Check: {1}", shiftCount + 1, ConvertBitArrayToString(check, check.Length));
                w = GetWeight(check);
                Console.WriteLine("[{0}] Weight: {1}", shiftCount + 1, w);
                shiftCount++;
            }
            bsRecived = Xor(bsRecived, check);
            Console.WriteLine("Xor: {0}", ConvertBitArrayToString(bsRecived, bsRecived.Length));
            bsRecived = ShiftRight(bsRecived, shiftCount);
            Console.WriteLine("Fixed: {0}", ConvertBitArrayToString(bsRecived, bsRecived.Length));

            Console.ReadKey();
        }
        public static void PrintValues(BitArray myList, int myWidth)
        {
            int i = myWidth;
            foreach (bool obj in myList)
            {
                if (i <= 0)
                {
                    i = myWidth;
                    Console.WriteLine();
                }
                i--;
                Console.Write( obj ? 1 : 0);
            }
            Console.WriteLine();
        }
        public static string ConvertBitArrayToString(BitArray myList, int myWidth)
        {
            string s = "";
            int i = myWidth;
            foreach (bool obj in myList)
            {
                if (i <= 0)
                {
                    i = myWidth;
                }
                i--;
                s += obj ? '1' : '0';
            }
            return s;
        }
        public static BitArray ConvertStringToBitArray(string s)
        {
            BitArray bs = new BitArray(s.Length);

            for (int i = 0; i < s.Length; i++)
                bs[i] = s[i] == '1' ? true : false;

            return bs;
        }
        public static BitArray SubBitArray(BitArray bs, int startIndex, int length)
        {
            if (startIndex + length > bs.Length || startIndex <= -1 || length <= -1)
                throw new Exception("Going beyond binary boundaries!");

            BitArray result = new BitArray(length);
            for (int i = startIndex, j = 0; i < startIndex + length; i++, j++)
                result[j] = bs[i];

            return result;
        }
        public static BitArray SubBitArray(BitArray bs, int startIndex)
        {
            if(startIndex <= -1)
                throw new Exception("Going beyond binary boundaries!");

            BitArray result = new BitArray(bs.Length - startIndex);
            for (int i = startIndex, j = 0; i < bs.Length; i++, j++)
                result[j] = bs[i];

            return result;
        }
        public static BitArray Trim(BitArray bs)
        {
            int count = 0;
            foreach (bool b in bs)
                if (!b)
                    count++;
                else
                    break;

            BitArray result = SubBitArray(bs, count, bs.Length - count);

            return result;
        }
        public static int ConvertBitArrayToInt32(BitArray bitArray)
        {
            bitArray = Reverse(bitArray);
            int value = 0;

            for (int i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i])
                    value += Convert.ToInt16(Math.Pow(2, i));
            }

            return value;
        }
        public static BitArray Reverse(BitArray array)
        {
            BitArray result = new BitArray(array.Length);
            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                bool bit = array[i];
                result[i] = array[length - i - 1];
                result[length - i - 1] = bit;
            }

            return result;
        }
        public static BitArray BitArrayAdd(BitArray bs, bool element)
        {
            BitArray result = new BitArray(bs.Length + 1);
            for (int i = 0; i < bs.Length; i++)
                result[i] = bs[i];
            result[bs.Length] = element;

            return result;
        }
        public static int GetWeight(BitArray bs)
        {
            int w = 0;
            foreach (bool b in bs)
                if (b)
                    w++;
            return w;
        }
        public static BitArray BitArrayAdd(BitArray bs, BitArray bs2)
        {
            int resultLength = bs.Length + bs2.Length;
            BitArray result = new BitArray(resultLength);
            for (int i = 0; i < bs.Length; i++)
                result[i] = bs[i];
            for (int i = bs.Length, j = 0; i < resultLength; i++, j++)
                result[i] = bs2[j];
            return result;
        }
        public static BitArray Xor(BitArray bs1, BitArray bs2)
        {
            if(bs1.Length > bs2.Length)
            {
                BitArray result = new BitArray(bs1.Length);
                for (int i = 0; i < bs1.Length - bs2.Length; i++)
                    result[i] = bs1[i];
                for (int i = bs1.Length - bs2.Length, j = 0; i < bs1.Length; i++, j++)
                    result[i] = bs1[i] ^ bs2[j];
                return result;
            }
            else if( bs1.Length < bs2.Length)
            {
                BitArray temp = bs2;
                bs2 = bs1;
                bs1 = temp;
                return Xor(bs1, bs2);
            }
            else
                return bs1.Xor(bs2);
        }
        public static BitArray ShiftLeft(BitArray bs)
        {
            BitArray result = (BitArray)bs.Clone();
            bool temp = result[0];
            for (int i = 0; i < result.Length - 1; i++)
                result[i] = result[i + 1];
            result[result.Length - 1] = temp;
            return result;
        }
        public static BitArray ShiftRight(BitArray bs)
        {
            BitArray result = (BitArray)bs.Clone();
            if (result.Length > 1)
            {
                var tmp = result[result.Length - 1];
                for (var i = result.Length - 1; i != 0; --i)
                    result[i] = result[i - 1];
                result[0] = tmp;
            }
            return result;
        }
        public static BitArray ShiftRight(BitArray bs, int count)
        {
            BitArray result = (BitArray)bs.Clone();
            for (int i = 0; i < count; i++)
                result = ShiftRight(result);
            return result;
        }
        public static BitArray RandomError(BitArray bs, int tryCount = 1)
        {
            Random random = new Random();
            BitArray result = (BitArray)bs.Clone();
            for (int i = 0; i < tryCount; i++)
            {
                int index = random.Next(0, bs.Length - 1);
                result[index] = !result[index];
            }
            return result;
        }
        public static BitArray Div(BitArray bs1, BitArray bs2)
        {
            BitArray bsSource = bs1;
            int sourceLength = bs1.Length;

            Console.WriteLine("Binary form: {0} divided by {1}",
                ConvertBitArrayToString(bs1, bs1.Length),
                ConvertBitArrayToString(bs2, bs2.Length));

            BitArray bsBy = bs2;
            BitArray temp = null;
            int byLength = bsBy.Length;
            int i = 0;

            temp = SubBitArray(bsSource, i, byLength);

            while (true)
            {
                if (temp[0])
                {
                    temp = temp.Xor(bsBy);
                }
                do
                {
                    i++;
                    temp = SubBitArray(temp, 1);
                    if (i + byLength - 1 < sourceLength)
                        temp = BitArrayAdd(temp, bsSource[i + byLength - 1]);
                    else
                        return temp;
                } while (!temp[0]);
            }

            return new BitArray(0);
        }
    }
}
