using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Lab3;
using Konsole;
using static System.ConsoleColor;

namespace Lab4
{
    class Computer
    {
        public const int NMax = 10;
        public Channel Channel
        {
            get
            {
                return channel;
            }
        }
        public byte[] Address
        {
            get
            {
                return address;
            }
        }
        public Status Status
        {
            get
            {
                return status;
            }
        }

        private Thread state;
        private Status status = Status.Offline;
        private Channel channel;
        private IConsole output;
        private byte[] address;
        private string name;
        private byte[] dataForSend = null;
        /// <summary>
        /// Attempt number.
        /// </summary>
        private int n;

        public Computer(string name, Channel channel, IConsole output, byte[] dataForSend = null)
        {
            this.name = name;
            this.dataForSend = dataForSend;
            this.address = Frame.GetRandomRealAddress();
            this.channel = channel;
            this.state = new Thread(Work);
            this.output = output;
        }

        public void Start()
        {
            state.Start();
            status = Status.Online;
        }

        private void Work()
        {
            Random random = new Random();
            while(true)
            {
                if (channel.Computers.Count < 2)
                    continue;
                byte[] dataForSend = new byte[this.dataForSend.Length];
                if (this.dataForSend == null)
                    random.NextBytes(dataForSend);
                else
                    dataForSend = this.dataForSend;
                IEnumerable<Computer> otherComputerInChannel = channel.Computers.Where(computer => computer.Address != address);
                Computer receiver = otherComputerInChannel.ToList()[random.Next(0, channel.Computers.Count - 1)];
                Frame frameForSend = new Frame(dataForSend, receiver.Address, address);
                Send(frameForSend);
                Thread.Sleep(random.Next(1, 1000));
            }
        }
        private void Stop()
        {
            state.Abort();
        }
        public int Send(Frame frame)
        {
            if (state.ThreadState != ThreadState.Running)
                this.status = Status.Online;
            int collisionWindow = channel.SignalFlowData * 2;
            bool isCollision;
            List<byte[]> collisions = new List<byte[]>();

            n = 0;
            do
            {
                while (channel.Busy) { }
                Frame frameForSend = (Frame)frame.Clone();
                channel.SetJam(false);
                channel.SendFrame(frameForSend);
                Thread.Sleep(collisionWindow);
                Frame frameReceive = channel.ReceiveFrame();
                isCollision = !frameForSend.GetFrame.SequenceEqual(frameReceive.GetFrame);
                if (isCollision)
                {
                    collisions.Add(frameReceive.Data);
                    //output.WriteLine(Red, "Collision detected!");
                    channel.SetJam(true);
                    Thread.Sleep(collisionWindow);
                    n++;
                    if (n > NMax)
                    {
                        if (state.ThreadState != ThreadState.Running)
                            this.status = Status.Offline;
                        return -1;
                    }
                    else
                        Thread.Sleep(GetSlotTime());
                }
                if (!isCollision)
                {
                    output.Write(Green, Encoding.ASCII.GetString(frame.Data));
                    if (frame.IsAutocomplate)
                        output.Write(DarkGray, " (is completed)");
                    output.Write(Gray, "    :   ");
                    if(collisions.Count == 0)
                        output.Write(Green, "not collisions");
                    else
                    {
                        foreach(byte[] collision in collisions)
                        {
                            output.Write(Red, "*");
                            //output.Write(DarkRed, Encoding.ASCII.GetString(collision));
                            //output.Write(" ");
                        }
                    }
                    output.WriteLine("");
                    //output.WriteLine("[{0} - {1}] Send \"{2}\" to {3}.",
                    //name,
                    //Encoding.UTF8.GetString(address),
                    //Encoding.UTF8.GetString(frameForSend.Data),
                    //Encoding.UTF8.GetString(frameForSend.DA)
                    //);
                }
            } while (isCollision);

            channel.FrameSended?.Invoke();
            if (state.ThreadState != ThreadState.Running)
                this.status = Status.Offline;
            return 0;
        }
        public void Wait()
        {
            state.Suspend();
            status = Status.Wait;
        }
        public void StartAndWait()
        {
            Start();
            Wait();
        }
        public void Received()
        {
            if(channel.DestinationAddress.SequenceEqual(address))
            {
                //Address of this computer.
                Frame receivedFrame = channel.ReceiveFrame();
                //output.WriteLine("[{0} - {1}] Receive \"{2}\" from {3}.",
                //    name,
                //    Encoding.UTF8.GetString(address),
                //    Encoding.UTF8.GetString(receivedFrame.Data),
                //    Encoding.UTF8.GetString(receivedFrame.SA)
                //    );
                CRC crc = new CRC();
                byte[] dataWithChech = crc.Decode(receivedFrame.CRC, receivedFrame.Length * 8);
                if(channel.GetJam())
                {
                    channel.SetJam(true);
                } 
                else if(!dataWithChech.SequenceEqual(receivedFrame.Data))
                {
                    channel.SetJam(true);
                }
                output.WriteLine(Encoding.ASCII.GetString(receivedFrame.Data));
            }
        }
        /// <summary>
        /// Get slot time (or as r).
        /// </summary>
        /// <returns></returns>
        private int GetSlotTime()
        {
            int timeBonus = 10;
            int k = Math.Min(n, 10);
            int slotTime = new Random().Next(0, Convert.ToInt32(Math.Pow(2, k))); // 0 <=r <= 2^k
            int factor = channel.SignalFlowData * 2 + channel.SignalFlowJam + timeBonus;
            return slotTime;
        }
        private static string GetRandomMacAddress()
        {
            var random = new Random();
            var buffer = new byte[6];
            random.NextBytes(buffer);
            var result = String.Concat(buffer.Select(x => string.Format("{0}:", x.ToString("X2"))).ToArray());
            return result.TrimEnd(':');
        }
    }
}
