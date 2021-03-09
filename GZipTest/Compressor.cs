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
    class Compressor : GZip
    {
        public Compressor(string input, string output) : base(input, output)
        {

        }

        public override void Launch()
        {
            Thread[] compressorThreads = new Thread[threadNumber];

            Thread reader = new Thread(new ThreadStart(Read));
            reader.Start();

            for (int i = 0; i < compressorThreads.Length; i++)
            {
                compressorThreads[i] = new Thread(new ThreadStart(Compress));
                compressorThreads[i].Start();
            }

            Thread writer = new Thread(new ThreadStart(Write));
            writer.Start();

            foreach (var th in compressorThreads)
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
                    int _dataPortionSize = 0;
                    byte[] dataPortion;
                    while (inFile.Position < inFile.Length)
                    {
                        if (inFile.Length - inFile.Position <= dataPortionSize)
                            _dataPortionSize = (int)(inFile.Length - inFile.Position);
                        else
                            _dataPortionSize = dataPortionSize;

                        if (_dataPortionSize <= 0) break;

                        dataPortion = new byte[_dataPortionSize];
                        inFile.Read(dataPortion, 0, _dataPortionSize);
                        queueReader.EnqueueForCompressing(dataPortion);
                    }
                }
                queueReader.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void Compress()
        {
            try
            {
                while (true)
                {
                    DataBlock block = queueReader.Dequeue();

                    if (block == null)
                        return;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(memoryStream, CompressionMode.Compress))
                        {
                            cs.Write(block.Data, 0, block.Data.Length);
                        }

                        DataBlock _out = new DataBlock(block.ID, memoryStream.ToArray());
                        queueWriter.EnqueueForWriting(_out);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//Возможно добавить стек к выводу
            }
        }

        void Write()
        {
            base.Write(true);
        }
    }
}
