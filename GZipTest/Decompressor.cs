using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    class Decompressor : GZip
    {
        public Decompressor(string input, string output) : base(input, output)
        {

        }

        int counter = 0;

        public override void Launch()
        {
            Console.Write("Decompressing...");

            Thread[] decompressorThreads = new Thread[threadNumber];

            Thread reader = new Thread(new ThreadStart(Read));
            reader.Start();

            for (int i = 0; i < decompressorThreads.Length; i++)
            {
                decompressorThreads[i] = new Thread(new ThreadStart(Decompress));
                decompressorThreads[i].Start();
            }

            Thread writer = new Thread(new ThreadStart(Write));
            writer.Start();

            foreach (var th in decompressorThreads)
                th.Join();
            while (!queueWriter.IsEmpty())
                Thread.Sleep(0);

            queueWriter.Stop();
        }

        protected override void Read()
        {
            try
            {
                using (FileStream inFile = new FileStream(inFileName, FileMode.Open, FileAccess.Read))
                {
                    while (inFile.Position < inFile.Length)
                    {
                        byte[] lengthBuffer = new byte[8];
                        inFile.Read(lengthBuffer, 0, lengthBuffer.Length);
                        int blockLength = BitConverter.ToInt32(lengthBuffer, 4);
                        byte[] compressedData = new byte[blockLength];
                        lengthBuffer.CopyTo(compressedData, 0);

                        inFile.Read(compressedData, 8, blockLength - 8);
                        int dataSize = BitConverter.ToInt32(compressedData, blockLength - 4);
                        byte[] emptyData = new byte[dataSize];

                        queueReader.EnqueueForWriting(new DataBlock(counter, emptyData, compressedData));
                        counter++;
                    }
                }
                queueReader.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void Decompress()
        {
            try
            {
                while (true)
                {
                    DataBlock block = queueReader.Dequeue();
                    if (block == null)
                        return;

                    using (MemoryStream ms = new MemoryStream(block.CompressedData))
                    {
                        using (GZipStream gzStream = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            gzStream.Read(block.Data, 0, block.Data.Length);
                            queueWriter.EnqueueForWriting(new DataBlock(block.ID, block.Data.ToArray()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void Write()
        {
            base.Write(false);
        }
    }
}
