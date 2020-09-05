using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Drawing;

namespace PicMan
{
    class Program
    {
        static bool shouldOverwite = false;
        static string outputStrFormat = "yyyy-MM";
        private static Regex metadataRegex = new Regex(":");

        static void Main(string[] args)
        {
            var rawSrcPath = @"D:\sftp\Vic_DCIM\";
            var destRootPath = @"F:\Photos";
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
                try
                {
                    var yrName = ParseYearFromFileName(file);
                    if (yrName == string.Empty)
                    {
                        Console.WriteLine($"Using filetime : {file}");
                        yrName = GetFileCreationTime(file);
                    }

                    var yrPath = $"{destRootPath}\\{yrName}\\";
                    if (!Directory.Exists(yrPath))
                    {
                        Directory.CreateDirectory(yrPath);
                    }

                    parsedCount++;
                    var destFile = yrPath + Path.GetFileName(file);
                    Copy(file, destFile, shouldOverwite);
                }
                catch(Exception ex)
                {
                    notParsedCount++;
                    Console.WriteLine($"Parse error : {file} {ex.Message}");
                    if (!Directory.Exists(assortedRootPath))
                    {
                        Directory.CreateDirectory(assortedRootPath);
                    }

                    var destFile = assortedRootPath + Path.GetFileName(file);
                    Copy(file, destFile, shouldOverwite);
                }

            }

            Console.WriteLine($"Success : {parsedCount}, Failed : {notParsedCount}");
        }

        private static void Copy(string srcFile, string destFile, bool overwrite)
        {
            if (!File.Exists(destFile) || overwrite) 
            {
                File.Copy(srcFile, destFile, overwrite); 
                Console.WriteLine($"Copied : {srcFile} -> {destFile}");
            }
            else
            {
                Console.WriteLine($"Skipping {destFile}");
            }
        }

        private static string GetFileCreationTime(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                PropertyItem propItem = myImage.GetPropertyItem(36867);
                string dateTaken = metadataRegex.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                var parsed = DateTime.Parse(dateTaken);
                return parsed.ToString(outputStrFormat);
            }
        }

        private static string ParseYearFromFileName(string file)
        {
            var name = Path.GetFileNameWithoutExtension(file);

            if (name.Split('_')[0].Length == 8)
            {
                // 20180310_211720
                name = name.Split('_')[0];
            }
            if (name.Split('_')[0] == "IMG" || name.Split('_')[0] == "VID")
            {
                // IMG_20180808_063039
                // VID_20191202_155556
                name = name.Split('_')[1];
            }
            if (name.Split('-')[0] == "IMG" || name.Split('-')[0] == "VID")
            {
                // IMG-20191130-WA0025
                name = name.Split('-')[1];
            }
            if (name.Split(' ')[0].Length == 10)
            {
                name = name.Split(' ')[0];
            }

            var parsed = DateTime.TryParseExact(name, new string[] { "yyyyMMdd", "yyyy-MM-dd" }, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime dt);
            if (parsed)
            {
                return dt.ToString(outputStrFormat);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
