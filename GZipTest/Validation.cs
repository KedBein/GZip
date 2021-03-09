using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace GZipTest
{
    public static class Validation
    {
        public static void StringReadValidation(string[] args)
        {

            if (args.Length == 0 || args.Length > 3)
            {
                throw new Exception("Пожалуйста введите агрументы по следующему шаблону:\n compress(decompress) [source file] [destination file].");
            }

            if (args[0].ToLower() != Program.STR_Compress && args[0].ToLower() != Program.STR_Decompress)
            {
                throw new Exception("Первым аргументом должен быть \"compress\" или \"decompress\".");
            }

            if (args[1].Length == 0)
            {
                throw new Exception("Не указано имя исходного файла.");
            }

            if (args[2].Length == 0)
            {
                throw new Exception("Не указано имя выходного файла.");
            }

            if (!File.Exists(args[1]))
            {
                throw new Exception("Исходный файл отсутствует.");
            }

            FileInfo _fileIn = new FileInfo(args[1]);
            FileInfo _fileOut = new FileInfo(args[2]);

            if (_fileIn.Extension == ".gz" && args[0] == Program.STR_Compress)
            {
                throw new Exception("Файл уже сжат.");
            }

            if (_fileIn.Extension != ".gz" && args[0] == Program.STR_Decompress)
            {
                throw new Exception("Исходный файл для разархивирования должен иметь .gz расширение.");
            }

            if (_fileOut.Extension == ".gz" && _fileOut.Exists)
            {
                throw new Exception("Исходящий файл уже существует. Пожалуйста выберите другое имя выходного файла.");
            }
        }
    }
}