using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KLine_CTA_N_MIN
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var monthDir in new DirectoryInfo(@"D:\Test\CTA_OUTPUT_FINAL").GetDirectories())
            {
                foreach(var dir in monthDir.GetDirectories())
                {
                    foreach(var file in dir.GetFiles())
                    {
                        Combiner c = new Combiner();
                        string outputDir = AppConfig.OUTPUT_ROOT_DIR + monthDir + "\\" + dir + "\\";
                        if(!Directory.Exists(outputDir))
                        {
                            Directory.CreateDirectory(outputDir);
                        }
                        c.Combine(file, outputDir + file.Name, 5);
                        Console.WriteLine("合成完成：" + outputDir + file.Name);
                    }
                }
            }
        }
    }
}
;