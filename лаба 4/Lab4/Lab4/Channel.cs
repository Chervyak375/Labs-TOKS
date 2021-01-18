using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lab4
{
    class Channel
    {
        public int SignalFlowData
        {
            get
            {
                return signalFlowData;
            }
        }
        public int SignalFlowJam
        {
            get
            {
                return signalFlowJam;
            }
        }
        public List<Computer> Computers
        {
            get
            {
                return computers.Where(computer => computer.Status != Status.Offline).ToList();
            }
        }
        public bool Busy
        {
            get
            {
                return busy;
            }
        }
        public byte[] DestinationAddress
        {
            get
            {
                return frame.DA;
            }
        }
        public Action FrameSended
        {
            get
            {
                return frameSended;
            }
        }

        private event Action frameSended;
        private List<Computer> computers = new List<Computer>();
        private Random random = new Random();
        private Frame frame;
        private bool busy;
        private bool jam;
        private int signalFlowData = 5;
        private int signalFlowJam = 5;
        public Channel() {}
        public void SendFrame(Frame frame)
        {
            busy = true;
            Thread.Sleep(signalFlowData);
            Frame f = (Frame)frame.Clone();
            if(random.NextDouble() < 0.60)
                Frame.Collision(ref f);
            this.frame = f;
        }
        public Frame ReceiveFrame()
        {
            Thread.Sleep(signalFlowData);
            busy = false;
            return frame;
        }
        public void SetJam(bool value)
        {
            Thread.Sleep(signalFlowJam);
            this.jam = value;
        }
        public bool GetJam()
        {
            Thread.Sleep(signalFlowJam);
            return jam;
        }
        public void AddRange(Computer[] coms)
        {
            foreach (Computer computer in coms)
                frameSended += computer.Received;
            Computers.AddRange(coms);
        }
        public void Add(Computer computer)
        {
            computers.Add(computer);
            frameSended += computer.Received;
        }
    }
}
