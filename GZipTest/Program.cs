using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest
{
    class Program
    {
        static public int Main(string[] args)
        {
            try
            {
                Validation.StringReadValidation(args);

                if (args[0].ToLower() == STR_Compress)
                    ZipProc = new Compressor(args[1], args[2]);
                if (args[0].ToLower() == STR_Decompress)
                    ZipProc = new Decompressor(args[1], args[2]);

                ZipProc.Launch();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error is occured!\n Method: {0}\n Error description {1}", ex.TargetSite, ex.Message);
                return 1;
            }
            return 0;
        }

        public static string STR_Compress = "compress";
        public static string STR_Decompress = "decompress";
        static GZip ZipProc;
    }
}