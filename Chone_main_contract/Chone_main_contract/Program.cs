using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Chone_main_contract
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> mainContractType = new List<string>();

            FileStream fs = new FileStream(@"", FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);

            string line = null;
            while ((line = sr.ReadLine()) != null)
            {
                mainContractType.Add(line);
            }

            string connStr = "Server=192.168.2.181;User ID=root;Password=123456;Database=CTAHisDBSPFT2017;CharSet=utf8";
            
        }

            
    }
}
    

