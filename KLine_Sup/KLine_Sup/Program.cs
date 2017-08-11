using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KLine_Sup
{
    class DATA
    {
        public string type;
        public DateTime date;
        public string cid;
        public string cname;
        public string fileName;
    }
    class Program
    {
        static void Main(string[] args)
        {
            

            FileStream fs = new FileStream(@"E:\主力合约.csv", FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            List<DATA> data = new List<DATA>();
            string line = null;
            while((line = sr.ReadLine())!=null)
            {
                string[] list = line.Split(',');
                DATA d = new DATA();
                d.type = list[0];
                d.date = Convert.ToDateTime(list[1]);
                d.cid = list[2];
                d.cname = list[3];
                d.fileName = list[4];


                if(d.cname == null || d.cname == "" )
                {
                    data.Add(d);
                }
            }

            fs.Close();
            sr.Close();




            List<string> contents = new List<string>();
            foreach (var item in data.OrderBy(d=>d.type).ThenBy(d=>d.date))
            {
                contents.Add(string.Format("{0},{1},{2},{3},{4}", item.type, item.date, item.cid, item.cname, item.fileName));
            }
            File.WriteAllLines(@"E:\数据缺失.csv", contents);
        }
    }
}
