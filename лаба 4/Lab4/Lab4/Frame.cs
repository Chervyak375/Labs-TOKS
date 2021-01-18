using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lab3;

namespace Lab4
{
    class Frame : ICloneable
    {
        public static readonly byte[] PREAMBLE = Enumerable.Repeat<byte>(170, 7).ToArray(); //10101010b * 7
        public const byte SFD = 171; // 10101011b
        public const int DATAMINLENGTH = 3;
        public const int DATAMAXLENGTH = 1500;

        public byte[] GetFrame
        {
            get
            {
                List<byte> f = new List<byte>();
                f.AddRange(preamble);
                f.Add(sfd);
                f.AddRange(da);
                f.AddRange(sa);
                f.AddRange(BitConverter.GetBytes(length));
                f.AddRange(data);
                if(pad!=null)
                    f.AddRange(pad);
                f.AddRange(fcs);
                if(extension != null)
                    f.AddRange(extension);
                return f.ToArray();
            }
        }
        public byte[] DA
        {
            get
            {
                return da;
            }
        }
        public byte[] SA
        {
            get
            {
                return sa;
            }
        }
        public byte[] CRC
        {
            get
            {
                return fcs;
            }
        }
        public ushort Length
        {
            get
            {
                return length;
            }
        }
        public byte[] Data
        {
            get
            {
                return data;
            }
        }
        public bool IsAutocomplate
        {
            get
            {
                return pad == null ? false : true;
            }
        }

        private readonly byte[] preamble = PREAMBLE;
        private const byte sfd = SFD;
        private byte[] da = new byte[6];
        private byte[] sa = new byte[6];
        private ushort length;
        private byte[] data;
        private byte[] pad;
        private byte[] fcs;
        private byte[] extension;
        private byte[] frame;

        /// <summary>
        /// For Clone.
        /// </summary>
        private Frame() { }
        public Frame(byte[] data, byte[] da, byte[] sa)
        {
            this.da = da;
            this.sa = sa;
            length = (ushort)data.Length;
            this.data = data;
            if(data.Length < DATAMINLENGTH)
                pad = Enumerable.Repeat<byte>(byte.MaxValue, (DATAMINLENGTH - data.Length)).ToArray();
            fcs = new CRC().Encode(this.data);
        }
        public static byte[] GetRandomAddress()
        {
            byte[] address = new byte[6];
            Random random = new Random();
            for (int i = 0; i < address.Length; i++)
                address[i] = (byte)random.Next(byte.MinValue, byte.MaxValue);
            return address;
        }
        public static byte[] GetRandomRealAddress()
        {
            char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            byte[] address = new byte[6];
            Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            for (int i = 0; i < address.Length; i++)
                address[i] = (byte)numbers[random.Next(0, numbers.Length - 1)];
            return address;
        }
        public static void Collision(ref Frame frame)
        {
            byte[] collisionData = new byte[frame.Length];
            for (int i = 0; i < frame.Length; i++)
                collisionData[i] = (byte)GeRandomtLetter(i * 2);
            frame.data = collisionData;
        }
        public static char GeRandomtLetter(int number)
        {
            string chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
            Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF + number);
            int num = rand.Next(0, chars.Length - 1);
            return chars[num];
        }
        public object Clone()
        {
            Frame frameClone = new Frame();
            frameClone.da = (byte[])da.Clone();
            frameClone.sa = (byte[])sa.Clone();
            frameClone.length = length;
            frameClone.data = (byte[])data.Clone();
            frameClone.pad = (byte[])pad?.Clone();
            frameClone.fcs = (byte[])fcs.Clone();
            frameClone.extension = (byte[])extension?.Clone();
            frameClone.frame = (byte[])frame?.Clone();
            return frameClone;
        }
    }
}
