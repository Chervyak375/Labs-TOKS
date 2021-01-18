using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class CRC
    {
        public List<string> Log
        {
            get
            {
                return log;
            }
        }

        int r;
        int S;
        int b;

        List<string> log = new List<string>();

        /// <summary>
        /// Construct.
        /// </summary>
        /// <param name="r">Number of errors detected.</param>
        /// <param name="b">Number of errors corrected.</param>
        public CRC(int r=1, int b=1)
        {
            this.r = r;
            this.b = b;
            this.S = r;
        }
        public string GetBasePoly(int inputLength)
        {
            int k = GetK(inputLength);
            BitArray basePoly = GetBasePolyByPower(k);
            return ConvertBitArrayToString(basePoly, basePoly.Length);
        }
        public string Encode(string input)
        {
            BitArray bsInput = ConvertStringToBitArray(input);
            BitArray bsResult = Encode(bsInput);
            return ConvertBitArrayToString(bsResult, bsResult.Length);
        }
        public BitArray Encode(BitArray bsInput)
        {
            log.Add(string.Format("Took input bits {0}", ConvertBitArrayToString(bsInput, bsInput.Length)));
            int k = GetK(bsInput.Length);
            BitArray basePoly = GetBasePolyByPower(k);
            log.Add(string.Format("Base polynomial: {0}", ConvertBitArrayToString(basePoly, basePoly.Length)));
            BitArray bsSource = BitArrayAdd(bsInput, new BitArray(k));
            log.Add(string.Format("Number of check bits is {0}", k));
            BitArray bsRemainder = Div(bsSource, basePoly);
            log.Add(string.Format("Division {0} by {1} and remainder (check bits) is {2}",
                ConvertBitArrayToString(bsSource, bsSource.Length),
                ConvertBitArrayToString(basePoly, basePoly.Length),
                ConvertBitArrayToString(bsRemainder, bsRemainder.Length)));
            BitArray bsResult = BitArrayAdd(bsInput, bsRemainder);
            log.Add(string.Format("CRC is {0}", ConvertBitArrayToString(bsResult, bsResult.Length)));
            return bsResult;
        }
        public byte[] Encode(byte[] bytesInput)
        {
            BitArray bsInput = new BitArray(bytesInput);
            BitArray bsResult = Encode(bsInput);
            return BitArrayToByteArray(bsResult);
        }
        public byte[] Decode(byte[] bytesTransfer, int dataLengthInBits)
        {
            BitArray bsRecived = new BitArray(bytesTransfer);
            bsRecived = Decode(bsRecived, dataLengthInBits);
            return BitArrayToByteArray(bsRecived);
        }
        public string Decode(string dataTransfer, int inputLength)
        {
            BitArray bsRecived = ConvertStringToBitArray(dataTransfer);
            bsRecived = Decode(bsRecived, inputLength);
            return ConvertBitArrayToString(bsRecived, bsRecived.Length);
        }
        public BitArray Decode(BitArray bsTransfer, int inputLength)
        {
            log.Add(string.Format("Took Transfer bits {0}", ConvertBitArrayToString(bsTransfer, bsTransfer.Length)));
            int k = GetK(inputLength);
            BitArray basePoly = GetBasePolyByPower(k);
            log.Add(string.Format("Base polynomial: {0}", ConvertBitArrayToString(basePoly, basePoly.Length)));
            BitArray bsRecived = bsTransfer;
            BitArray check = Div(bsRecived, basePoly);
            log.Add(string.Format("Division {0} by {1} and remainder (check bits) is {2}",
                ConvertBitArrayToString(bsRecived, bsRecived.Length),
                ConvertBitArrayToString(basePoly, basePoly.Length),
                ConvertBitArrayToString(check, check.Length)));
            int w = GetWeight(check);
            int shiftCount = 0;
            if (w > S)
                log.Add("Error detected!");
            while (w > S)
            {
                log.Add(string.Format("Check bits: {0} (weight {1} > S {2} (fixing count))", ConvertBitArrayToString(check, check.Length), w, S));
                BitArray bsRecivedOld1 = (BitArray)bsRecived.Clone();
                bsRecived = ShiftLeft(bsRecived);
                log.Add(string.Format("One shift to left. {0} << {1}",
                    ConvertBitArrayToString(bsRecivedOld1, bsRecivedOld1.Length),
                    ConvertBitArrayToString(bsRecived, bsRecived.Length)));
                check = Div(bsRecived, basePoly);
                log.Add(string.Format("Division {0} by {1} and remainder (check bits) is {2}",
                    ConvertBitArrayToString(bsRecived, bsRecived.Length),
                    ConvertBitArrayToString(basePoly, basePoly.Length),
                    ConvertBitArrayToString(check, check.Length)));
                w = GetWeight(check);
                shiftCount++;
            }
            log.Add(string.Format("Check bits: {0} (weight {1} == S {2} (fixing count))", ConvertBitArrayToString(check, check.Length), w, S));
            BitArray bsRecivedOld = (BitArray)bsRecived.Clone();
            bsRecived = Xor(bsRecived, check);
            if (shiftCount > 0)
                log.Add(string.Format("Error fix: {0} mod 2 {1}",
                    ConvertBitArrayToString(bsRecivedOld, bsRecivedOld.Length),
                    ConvertBitArrayToString(bsRecived, bsRecived.Length)));
            else
                log.Add("No error!");
            bsRecivedOld = (BitArray)bsRecived.Clone();
            bsRecived = ShiftRight(bsRecived, shiftCount);
            if (shiftCount > 0)
                log.Add(string.Format("Shifts back to right: {0} >>({1}) {2}",
                    ConvertBitArrayToString(bsRecivedOld, bsRecivedOld.Length),
                    shiftCount,
                    ConvertBitArrayToString(basePoly, bsRecived.Length)));
            bsRecived = SubBitArray(bsRecived, 0, inputLength);
            log.Add(string.Format("Extracting information bits: {0}", ConvertBitArrayToString(bsRecived, bsRecived.Length)));
            return bsRecived;
        }
        /// <summary>
        /// Return code distance value.
        /// </summary>
        /// <returns></returns>
        public int GetDmin() => r + 1;
        /// <summary>
        /// Return number of check digits.
        /// </summary>
        /// <param name="m">Input length.</param>
        /// <returns></returns>
        public int GetK(int m)
        {
            return (int)Math.Ceiling(
                Math.Log(
                        (double)(int)(
                            m + 1 + Math.Ceiling(Math.Log((double)(m + 1), 2))
                        ), 2
                    )
                );
        }
        public int GetCRCLength(byte[] input)
        {
            BitArray bsInput = new BitArray(input);
            int k = GetK(bsInput.Length);
            int crcLength = bsInput.Length + k;
            return crcLength;
        }
        private static BitArray GetBasePolyByPower(int k)
        {
            return ConvertStringToBitArray(new string[] {
                "11",
                "111",
                "1011",
                "10011",
                "100101",
                "1000011",
                "10001001",
                "111100111",
                "1000101101",
                "10000011011",
                "111100001011",
                "1101110100111"
            }[k - 1]);
        }
        static void PrintValues(BitArray myList, int myWidth)
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
                Console.Write(obj ? 1 : 0);
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
        static BitArray SubBitArray(BitArray bs, int startIndex, int length)
        {
            if (startIndex + length > bs.Length || startIndex <= -1 || length <= -1)
                throw new Exception("Going beyond binary boundaries!");

            BitArray result = new BitArray(length);
            for (int i = startIndex, j = 0; i < startIndex + length; i++, j++)
                result[j] = bs[i];

            return result;
        }
        static BitArray SubBitArray(BitArray bs, int startIndex)
        {
            if (startIndex <= -1)
                throw new Exception("Going beyond binary boundaries!");

            BitArray result = new BitArray(bs.Length - startIndex);
            for (int i = startIndex, j = 0; i < bs.Length; i++, j++)
                result[j] = bs[i];

            return result;
        }
        static BitArray Trim(BitArray bs)
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
        static BitArray Reverse(BitArray array)
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
        static BitArray BitArrayAdd(BitArray bs, bool element)
        {
            BitArray result = new BitArray(bs.Length + 1);
            for (int i = 0; i < bs.Length; i++)
                result[i] = bs[i];
            result[bs.Length] = element;

            return result;
        }
        static int GetWeight(BitArray bs)
        {
            int w = 0;
            foreach (bool b in bs)
                if (b)
                    w++;
            return w;
        }
        static BitArray BitArrayAdd(BitArray bs, BitArray bs2)
        {
            int resultLength = bs.Length + bs2.Length;
            BitArray result = new BitArray(resultLength);
            for (int i = 0; i < bs.Length; i++)
                result[i] = bs[i];
            for (int i = bs.Length, j = 0; i < resultLength; i++, j++)
                result[i] = bs2[j];
            return result;
        }
        static BitArray Xor(BitArray bs1, BitArray bs2)
        {
            if (bs1.Length > bs2.Length)
            {
                BitArray result = new BitArray(bs1.Length);
                for (int i = 0; i < bs1.Length - bs2.Length; i++)
                    result[i] = bs1[i];
                for (int i = bs1.Length - bs2.Length, j = 0; i < bs1.Length; i++, j++)
                    result[i] = bs1[i] ^ bs2[j];
                return result;
            }
            else if (bs1.Length < bs2.Length)
            {
                BitArray temp = bs2;
                bs2 = bs1;
                bs1 = temp;
                return Xor(bs1, bs2);
            }
            else
                return bs1.Xor(bs2);
        }
        static BitArray ShiftLeft(BitArray bs)
        {
            BitArray result = (BitArray)bs.Clone();
            bool temp = result[0];
            for (int i = 0; i < result.Length - 1; i++)
                result[i] = result[i + 1];
            result[result.Length - 1] = temp;
            return result;
        }
        static BitArray ShiftRight(BitArray bs)
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
        static BitArray ShiftRight(BitArray bs, int count)
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
        static BitArray Div(BitArray bs1, BitArray bs2)
        {
            BitArray bsSource = bs1;
            int sourceLength = bs1.Length;

            //Console.WriteLine("Binary form: {0} divided by {1}",
            //    ConvertBitArrayToString(bs1, bs1.Length),
            //    ConvertBitArrayToString(bs2, bs2.Length));

            BitArray bsBy = bs2;
            BitArray temp = null;
            int byLength = bsBy.Length;
            int i = 0;
            int bsByDec = ConvertBitArrayToInt32(bs2);

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
        public static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
    }
}
