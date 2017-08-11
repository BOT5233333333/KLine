using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace DataChecker
{
    class Program
    {
        /// <summary>
        /// k线数据结构体
        /// </summary>
        struct DATA_KLINE
        {
            /// <summary>
            /// 合约代码
            /// </summary>
            public string contractid;
            /// <summary>
            /// 合约名
            /// </summary>
            public string contractname;
            /// <summary>
            /// 交易时间
            /// </summary>
            public DateTime tdatetime;
            /// <summary>
            /// 最高价
            /// </summary>
            public double highpx;
            /// <summary>
            /// 最低价
            /// </summary>
            public double lowpx;
            /// <summary>
            /// 开盘价
            /// </summary>
            public double openpx;
            /// <summary>
            /// 收盘价
            /// </summary>
            public double closepx;

            public DATA_KLINE(string contractid, string contractname, DateTime tdatetime, double highpx, double lowpx, double openpx, double closepx)
            {
                this.contractid = contractid;
                this.contractname = contractname;
                this.tdatetime = tdatetime;
                this.highpx = highpx;
                this.lowpx = lowpx;
                this.openpx = openpx;
                this.closepx = closepx;
            }

            public DATA_KLINE(DATA_KLINE data)
            {
                this.contractid = data.contractid;
                this.contractname = data.contractname;
                this.tdatetime = data.tdatetime;
                this.highpx = data.highpx;
                this.lowpx = data.lowpx;
                this.openpx = data.openpx;
                this.closepx = data.closepx;
            }
        }

        static void Main(string[] args)
        {
            List<DATA_KLINE> myData = new List<DATA_KLINE>();
            FileStream fs_mine = new FileStream(@"E:\CTA_OUTPUT_FINAL\201701\a\a201701.csv", FileMode.Open);
            StreamReader sr_mine = new StreamReader(fs_mine, Encoding.UTF8);
            string line_mine = null;
            while((line_mine = sr_mine.ReadLine())!=null)
            {
                string[] list = line_mine.Split(',');
                myData.Add(new DATA_KLINE(
                    list[0]
                    , list[1]
                    , Convert.ToDateTime(list[2])
                    , Convert.ToDouble(list[3])
                    , Convert.ToDouble(list[4])
                    , Convert.ToDouble(list[5])
                    , Convert.ToDouble(list[6])));
            }
            fs_mine.Close();
            sr_mine.Close();

            FileStream fs = new FileStream(@"E:\数据检测\A_1m_data.csv", FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            string line = null;
            sr.ReadLine();
            int count = 0;
            while ((line=sr.ReadLine())!=null)
            {
                string[] list = line.Split(',');
                DateTime dt = Convert.ToDateTime(list[0]);
                var dataIndex = myData.FindIndex(item => item.tdatetime == dt);
                if (dataIndex >=0)
                {
                    DATA_KLINE dk = myData[dataIndex];
                    try
                    {
                        if (Convert.ToDouble(list[1]) != dk.openpx || Convert.ToDouble(list[2]) != dk.highpx || Convert.ToDouble(list[3]) != dk.lowpx || Convert.ToDouble(list[4]) != dk.closepx)
                        {
                            Console.WriteLine("----" + "错误类型：数据对比出错\n"+ "合约代码：" + dk.contractid+ "\n对比数据：" + line + "\n" + string.Format("我的数据：{0},{1},{2},{3},{4}", dk.tdatetime.ToString("yyyy-MM-dd HH:mm:ss"), dk.openpx, dk.highpx, dk.lowpx, dk.closepx) );
                            Log.AppendAllLines(new string[5] { "----", "错误类型：数据对比出错", "合约代码：" + dk.contractid, "对比数据：" + line, string.Format("我的数据：{0},{1},{2},{3},{4}", dk.tdatetime.ToString("yyyy-MM-dd HH:mm:ss"), dk.openpx, dk.highpx, dk.lowpx, dk.closepx) });
                        }
                    }
                    catch(FormatException )
                    {

                    }
                    
                }
                if(++count % 500 == 0)
                {
                    Console.WriteLine(count + " Lines Checked");
                }
                
            }
            
        }
    }
}
