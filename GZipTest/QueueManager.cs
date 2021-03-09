using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    public class DataBlock
    {
        private int id;
        private byte[] data;
        private byte[] compressedData;

        public int ID { get { return id; } }
        public byte[] Data { get { return data; } }
        public byte[] CompressedData { get { return compressedData; } }


        public DataBlock(int id, byte[] data) : this(id, data, new byte[0])
        {

        }

        public DataBlock(int id, byte[] data, byte[] compressedData)
        {
            this.id = id;
            this.data = data;
            this.compressedData = compressedData;
        }

    }

    public class QueueManager
    {
        private object locker = new object();
        Queue<DataBlock> queue = new Queue<DataBlock>();
        bool isDead = false;
        private int blockId = 0;
        private int maxCountBlocks = 50;

        public QueueManager(int _maxCountBlocks)
        {
            maxCountBlocks = _maxCountBlocks;
        }

        public void EnqueueForWriting(DataBlock _block)
        {
            while (queue.Count >= maxCountBlocks)
                Thread.Sleep(0);
            int id = _block.ID;
            lock (locker)
            {
                if (isDead)
                    throw new InvalidOperationException("Queue already stopped");

                while (id != blockId)
                    Monitor.Wait(locker);

                queue.Enqueue(_block);
                blockId++;
                Monitor.PulseAll(locker);
            }
        }

        public void EnqueueForCompressing(byte[] buffer)
        {
            while (queue.Count >= maxCountBlocks)
                Thread.Sleep(0);
            lock (locker)
            {
                if (isDead)
                    throw new InvalidOperationException("Queue already stopped");

                DataBlock _block = new DataBlock(blockId, buffer);
                queue.Enqueue(_block);
                blockId++;
                Monitor.PulseAll(locker);
            }
        }

        public DataBlock Dequeue()
        {
            lock (locker)
            {
                while (queue.Count == 0 && !isDead)
                    Monitor.Wait(locker);

                if (queue.Count == 0)
                    return null;

                return queue.Dequeue();
            }
        }

        public int Count()
        {
            return queue.Count;
        }

        public bool IsEmpty()
        {
            return queue.Count == 0;
        }

        public void Stop()
        {
            lock (locker)
            {
                isDead = true;
                Monitor.PulseAll(locker);
            }
        }
    }
}