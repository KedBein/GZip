using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public abstract class GZip
    {
        protected string inFileName, outFile;
        protected static int threadNumber = Environment.ProcessorCount;

        protected int dataPortionSize = 10000000;
        private static int maxReaderBlocks = 45;
        private static int maxWriterBlocks = 30;
        protected QueueManager queueReader = new QueueManager(maxReaderBlocks);
        protected QueueManager queueWriter = new QueueManager(maxWriterBlocks);

        public GZip(string input, string output)
        {
            this.inFileName = input;
            this.outFile = output;
        }

        public abstract void Launch();
        protected abstract void Read();

        protected void Write(bool isCompressing)
        {
            try
            {
                string newFileName;
                if (isCompressing)
                    newFileName = outFile + ".gz";
                else
                    newFileName = outFile;

                using (FileStream fileCompressed = new FileStream(newFileName, FileMode.Append))
                {
                    while (true)
                    {
                        DataBlock block = queueWriter.Dequeue();
                        if (block == null)
                            return;

                        if (isCompressing)
                            BitConverter.GetBytes(block.Data.Length).CopyTo(block.Data, 4);
                        fileCompressed.Write(block.Data, 0, block.Data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
