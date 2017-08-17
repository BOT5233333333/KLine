using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KLine_CTA_N_MIN
{
    class Combiner
    {
        public void Combine(FileInfo file, string outputPath, uint N_MIN)
        {
            List<KLineDataUnit> data = new List<KLineDataUnit>();
            FileStream fs = file.OpenRead();
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            string line = null;
            while((line = sr.ReadLine())!=null)
            {
                string[] list = line.Split(',');
                DateTime dt = Convert.ToDateTime(list[2]);
                long  group = dt.Ticks / 10000000;
                group /= 60*N_MIN;
                data.Add(new KLineDataUnit(
                    list[0]
                    , list[1]
                    , dt
                    , Convert.ToDouble(list[3])
                    , Convert.ToDouble(list[4])
                    , Convert.ToDouble(list[5])
                    , Convert.ToDouble(list[6])
                    , group
                    ));
            }
            fs.Dispose();
            sr.Dispose();


            List<KLineDataUnit> klinedata = new List<KLineDataUnit>();
            foreach(var g in data.OrderBy(item=>item.Tdatetime).GroupBy(item=>item.group))
            {
                klinedata.Add(new KLineDataUnit(
                    g.First().ContractID
                    , g.First().ContractName
                    , g.First().Tdatetime
                    , g.Max(item => item.Highpx)
                    , g.Min(item => item.Lowpx)
                    , g.First().Openpx
                    , g.Last().Closepx
                    , 0 ));
            }

            Print(klinedata, outputPath);

        }

        private void Print(List<KLineDataUnit> data, string outputPath)
        {
            List<string> contents = new List<string>();
            foreach(var d in data)
            {
                contents.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}"
                    , d.ContractID
                    , d.ContractName
                    , d.Tdatetime
                    , d.Highpx
                    , d.Lowpx
                    , d.Openpx
                    , d.Closepx));
            }
            File.WriteAllLines(outputPath, contents);
        }
        
        
    }
}
