using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicMan
{
    class Program
    {
        static void Main(string[] args)
        {
            var rawSrcPath = @"D:\Raw\t";
            var destRootPath = @"F:\Photos\";
            var assortedRootPath = @"F:\Photos\Assorted\";

            GroupByYear(rawSrcPath, destRootPath, assortedRootPath);
        }

        private static void GroupByYear(string rawSrcPath, string destRootPath, string assortedRootPath)
        {
            var allFiles = Directory.GetFiles(rawSrcPath);
            var notParsedCount = 0;
            var parsedCount = 0;

            foreach(var file in allFiles)
            {
                var yrName = ParseYear(file);
                if (yrName == string.Empty)
                {
                    Console.WriteLine($"Parse error : {file}");
                    notParsedCount++;
                    if (!Directory.Exists(assortedRootPath))
                    {
                        Directory.CreateDirectory(assortedRootPath);
                    }

                    var destFile = assortedRootPath + Path.GetFileName(file);
                    Copy(file, destFile, overwrite: false);
                }
                else
                {
                    parsedCount++;
                    var yrPath = $"{destRootPath}\\{yrName}\\";
                    if (!Directory.Exists(yrPath))
                    {
                        Directory.CreateDirectory(yrPath);
                    }

                    var destFile = yrPath + Path.GetFileName(file);
                    Copy(file, destFile, overwrite: false);
                }
            }

            Console.WriteLine($"Success : {parsedCount}, Failed : {notParsedCount}");
        }

        private static void Copy(string srcFile, string destFile, bool overwrite)
        {
            if (!File.Exists(destFile) || overwrite) // Dont overwrite
            {
                File.Copy(srcFile, destFile);
                Console.WriteLine($"Copied : {srcFile} -> {destFile}");
            }
            else
            {
                Console.WriteLine($"Skipping {destFile}");
            }
        }


        private static string ParseYear(string file)
        {
            var name = Path.GetFileNameWithoutExtension(file);

            if (name.Split('_')[0].Length == 8)
            {
                name = name.Split('_')[0];
            }
            if (name.Split(' ')[0].Length == 10)
            {
                name = name.Split(' ')[0];
            }

            var parsed = DateTime.TryParseExact(name, new string[] { "yyyyMMdd", "yyyy-MM-dd" }, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime dt);
            if (parsed)
            {
                return dt.ToString("yyyy-MM");
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
