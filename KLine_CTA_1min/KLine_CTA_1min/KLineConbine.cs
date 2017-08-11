using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace KLine_CTA_1min
{
    /// <summary>
    /// tick级数据文件中的一个数据条目
    /// </summary>
    struct DATA_UNIT
    {
        public string contractid;//合约代码
        public string contractname;//合约名
        public DateTime tdatetime;//交易时间
        public double lastpx;//最新价
        public DateTime group;//分组凭据

        public DATA_UNIT(string contractid, string contractname, DateTime tdatetime, double lastpx, DateTime group)
        {
            this.contractid = contractid;
            this.contractname = contractname;
            this.tdatetime = tdatetime;
            this.lastpx = lastpx;
            this.group = group;
        }
    }

    class KLineConbine
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

        /// <summary>
        /// 一天中的交易时间,，Dictionary的key为开始时间，value为结束时间，时间必须按顺序且不交叉地初始化
        /// </summary>
        static Dictionary<DateTime, DateTime> TransactionHour = new Dictionary<DateTime, DateTime>{
                  {new DateTime(2017,1,1,0,0,0),new DateTime(2017,1,1,2,30,0) }
            ,{new DateTime(2017,1,1,9,0,0),new DateTime(2017,1,1,10,15,0)}
            ,{new DateTime(2017,1,1,10,30,0),new DateTime(2017,1,1,11,30,0)}
            ,{new DateTime(2017,1,1,13,30,0),new DateTime(2017,1,1,15,0,0)}
            ,{new DateTime(2017,1,1,21,0,0),new DateTime(2017,1,1,23,59,59,999)}};

        /// <summary>
        /// 根据修正后的合约信息表（AppConfig.MAIN_CONTRACT_INFO_OUTPUT_FIX_PATH）合成一分钟级数据
        /// 并输出到AppConfig.DATA_OUTPUT_ROOT_DIR目录
        /// </summary>
        public static void Conbine_1MIin()
        {
            Console.WriteLine("开始合成分钟级数据");

            List<MainContract> mcs = new List<MainContract>();
            FileStream fs_info = new FileStream(AppConfig.MAIN_CONTRACT_INFO_OUTPUT_FIX_PATH, FileMode.Open);
            StreamReader sr_info = new StreamReader(fs_info, Encoding.UTF8);
            string line_info = null;
            while((line_info=sr_info.ReadLine())!=null)
            {
                string[] list_info = line_info.Split(',');
                mcs.Add(new MainContract(
                    list_info[0]
                    , Convert.ToDateTime(list_info[1])
                    , list_info[2]
                    , list_info[3]
                    , new FileInfo(AppConfig.DATA_SOURCE_ROOT_DIR + Convert.ToDateTime(list_info[1]).ToString("yyyyMM") + "\\" + list_info[0] + "\\" + list_info[4])
                    , 0));
            }
            fs_info.Close();
            sr_info.Close();

            foreach (var mc in mcs)
            {
                List<DATA_UNIT> dataList = new List<DATA_UNIT>();

                //默认文件中的时间是按顺序
                FileStream fs = mc.file.OpenRead();
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] data = line.Split(',');
                    if (data.Length != 55)
                    {
                        //数据异常，输出Log
                        Log.AppendAllLines(new string[5] { "-------", "错误信息：列数错误", "出错文件：" + mc.file.FullName, "出错列：" + line, "-------", });
                        continue;
                    }

                    DateTime tdatetime = Convert.ToDateTime(data[2]);

                    double lastpx = Convert.ToDouble(data[3]);
                    DateTime group = new DateTime(tdatetime.Year, tdatetime.Month, tdatetime.Day, tdatetime.Hour, tdatetime.Minute, 0);
                    if (isInLastTransactionHour(group))
                    {
                        //将最后一时刻归并到其上一分钟之中
                        group = group.AddMinutes(-1);
                    }

                    dataList.Add(new DATA_UNIT(data[0], data[1], tdatetime, lastpx, group));
                }
                fs.Dispose();
                sr.Dispose();

                //合成K线分钟级数据
                List<DATA_KLINE> klineData = new List<DATA_KLINE>();
                foreach (var g in dataList.OrderBy(item => item.tdatetime).GroupBy(item => item.group))
                {
                    if (!isInTransactionHour(g.Key)) { continue; }

                    klineData.Add(new DATA_KLINE(
                        g.First().contractid
                        , g.First().contractname
                        , g.First().group
                        , g.Max(item => item.lastpx)
                        , g.Min(item => item.lastpx)
                        , g.First().lastpx
                        , g.Last().lastpx));
                }

                //检查是否在交易时间的所有分钟内都有数据，若某一分钟内无数据，则由上一分钟的收盘价填补此分钟的数据
                //若保证每一分钟内都有数据，可将此函数注释
                klineData = Filling(klineData);

                //输出csv文件
                string outputDir = AppConfig.DATA_OUTPUT_ROOT_DIR + mc.date.ToString("yyyyMM") + "\\" + mc.type + "\\";
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                List<string> contents = new List<string>();
                foreach (var data in klineData)
                {
                    contents.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}"
                        , data.contractid
                        , data.contractname
                        , data.tdatetime
                        , data.highpx
                        , data.lowpx
                        , data.openpx
                        , data.closepx));
                }
                File.WriteAllLines(outputDir + mc.file.Name, contents);
                Console.WriteLine("合并完成:" + outputDir + mc.file.Name);
            }
        }

        /// <summary>
        /// 修正(AppConfig.MAIN_CONTRACT_INFO_OUTPUT_PATH即存有主力合约相关信息的文件，并且输出修正后的主力合约信息文件
        /// </summary>
        public static void FixMainContractInfo()
        {
            List<MainContract> infos = new List<MainContract>();

            FileStream fs_info = new FileStream(AppConfig.MAIN_CONTRACT_INFO_OUTPUT_PATH, FileMode.Open);
            StreamReader sr_info = new StreamReader(fs_info, Encoding.UTF8);
            string line_info = null;
            while ((line_info = sr_info.ReadLine()) != null)
            {
                string[] list = line_info.Split(',');
                infos.Add(new MainContract(
                    list[0]
                    , Convert.ToDateTime(list[1])
                    , list[2]
                    , list[3]
                    , new FileInfo(AppConfig.DATA_SOURCE_ROOT_DIR + Convert.ToDateTime(list[1]).ToString("yyyyMM") + "\\" + list[0] + "\\" + list[4])
                    , 0));
            }
            fs_info.Dispose();
            fs_info.Dispose();

            List<string> contents = new List<string>();
            foreach(var g in infos.GroupBy(item=>item.type))
            {
                string curMainContractID = g.First().contractID;
                for (int i=0; i<g.Count();++i)
                {
                    string fileDir_new = "";

                    if (g.ElementAt(i).contractID!=curMainContractID)
                    {
                        //找到切换日
                        --i;
                        MainContract mc = new MainContract(g.ElementAt(i).type
                            , g.ElementAt(i + 1).date
                            , g.ElementAt(i).contractID
                            , g.ElementAt(i).contractName
                            , new FileInfo(g.ElementAt(i + 1).file.FullName.Replace(g.ElementAt(i + 1).contractID, g.ElementAt(i).contractID))
                            , 0);

                        contents.Add(string.Format("{0},{1},{2},{3},{4}"
                            , mc.type
                            , mc.date
                            , mc.contractID
                            , mc.contractName
                            , mc.file.Name));

                        fileDir_new = mc.file.DirectoryName.Replace(AppConfig.DATA_SOURCE_ROOT_DIR, AppConfig.CTA_MAIN_CONTRACT_TEMP_DIR);

                        //if (!Directory.Exists(fileDir_new))
                        //{
                        //    Directory.CreateDirectory(fileDir_new);
                        //}
                        //mc.file.CopyTo(fileDir_new + "\\" + mc.file.Name, true);
                        Console.WriteLine("完成修正：" + fileDir_new + "\\" + mc.file.Name);

                        curMainContractID = g.ElementAt(++i).contractID;
                        
                    }
                    contents.Add(string.Format("{0},{1},{2},{3},{4}", g.ElementAt(i).type, g.ElementAt(i).date, g.ElementAt(i).contractID, g.ElementAt(i).contractName, g.ElementAt(i).file.Name));

                    fileDir_new = g.ElementAt(i).file.DirectoryName.Replace(AppConfig.DATA_SOURCE_ROOT_DIR, AppConfig.CTA_MAIN_CONTRACT_TEMP_DIR);

                    //if (!Directory.Exists(fileDir_new))
                    //{
                    //    Directory.CreateDirectory(fileDir_new);
                    //}
                    //g.ElementAt(i).file.CopyTo(fileDir_new + "\\" + g.ElementAt(i).file.Name, true);
                    Console.WriteLine("完成修正：" + fileDir_new + "\\" + g.ElementAt(i).file.Name);
                }
            }
            //输出修正后的信息
            File.WriteAllLines(AppConfig.MAIN_CONTRACT_INFO_OUTPUT_FIX_PATH, contents);
        }

        /// <summary>
        /// 填充交易时间内的每一分钟数据，若某一分钟缺失，该分钟的开盘、收盘、最高、最低价都是上一分钟的收盘价。
        /// 若klineData的元素数量和一开始计算得出的应有分钟数相同，则直接返回klineData
        /// </summary>
        /// <param name="klineData">已根据tick数据文件已有数据生成的一分钟k线数据，一个元素代表一分钟的数据，源文件中缺失的分钟不在其中</param>
        /// <returns>根据KLineConbine.TransactionHour（交易时间）生成的填充好的一分钟k线数据，一个元素代表一分钟的数据</returns>
        private static List<DATA_KLINE> Filling(List<DATA_KLINE> klineData)
        {
            //如果分钟数与计算出的应有分钟数相同则直接返回
            double numMin = 0;
            foreach(var th in KLineConbine.TransactionHour)
            {
                numMin += (th.Value - th.Key).TotalMinutes;
            }
            if(klineData.Count == Convert.ToInt16(numMin))
            {
                return klineData;
            }

            int i = 0;
            //开始检查
            foreach(var th in KLineConbine.TransactionHour)
            {
                for(DateTime timeCursor = th.Key; timeCursor.TimeOfDay<th.Value.TimeOfDay; timeCursor = timeCursor.AddMinutes(1), ++i)
                {
                    if (i >= klineData.Count)
                    {
                        //末尾空缺特殊处理
                        if (timeCursor.TimeOfDay == new TimeSpan(21,0,0))
                        {
                            //夜盘第一项缺失
                            klineData.Add(new DATA_KLINE
                                (klineData.Last().contractid
                                , klineData.Last().contractname
                                , new DateTime(klineData.Last().tdatetime.Year, klineData.Last().tdatetime.Month, klineData.Last().tdatetime.Day, th.Key.Hour, th.Key.Minute, th.Key.Second)
                                , -1
                                , -1
                                , -1
                                , -1));
                            Log.AppendAllLines(new string[4] { "-------", "错误信息：夜盘第一项数据缺失，夜盘数据可能整体缺失", "出错文件：" + klineData.Last().contractid+klineData.Last().tdatetime.ToString("_yyyyMMdd") + ".csv", "-------", });
                        }
                        else
                        {
                            klineData.Add(new DATA_KLINE
                            (klineData.Last().contractid
                            , klineData.Last().contractname
                            , klineData.Last().tdatetime.AddMinutes(1)
                            , klineData.Last().closepx
                            , klineData.Last().closepx
                            , klineData.Last().closepx
                            , klineData.Last().closepx));
                        }
                    }
                    else if (klineData[i].tdatetime.TimeOfDay!=timeCursor.TimeOfDay)
                    {
                        if(timeCursor == th.Key)
                        {
                            //交易时间段第一项缺失
                            if(i == 0)
                            {
                                //日盘第一项缺失(9:00)
                                klineData.Insert(i, new DATA_KLINE
                                    (klineData[i].contractid
                                    , klineData[i].contractname
                                    , new DateTime(klineData[i].tdatetime.Year, klineData[i].tdatetime.Month, klineData[i].tdatetime.Day,th.Key.Hour, th.Key.Minute,th.Key.Second)
                                    , -1
                                    , -1
                                    , -1
                                    , -1));
                                Log.AppendAllLines(new string[4] { "-------", "错误信息：日盘第一项数据缺失，日盘数据可能整体缺失", "出错文件：" + klineData[i].contractid + klineData[i].tdatetime.ToString("_yyyyMMdd") + ".csv", "-------", });
                            }
                            else
                            {
                                klineData.Insert(i--, new DATA_KLINE
                                    (klineData[i].contractid
                                    , klineData[i].contractname
                                    , new DateTime(klineData[i].tdatetime.Year, klineData[i].tdatetime.Month, klineData[i].tdatetime.Day, th.Key.Hour, th.Key.Minute, th.Key.Second)
                                    , klineData[i].closepx
                                    , klineData[i].closepx
                                    , klineData[i].closepx
                                    , klineData[i++].closepx));
                            }
                        }
                        else
                        {
                            //klineData[i].tdatetime 只会比timeCursor大
                            //根据上一分钟的收盘价格插入缺失的分钟
                            klineData.Insert(i--, new DATA_KLINE
                            (klineData[i].contractid
                            , klineData[i].contractname
                            , klineData[i].tdatetime.AddMinutes(1)
                            , klineData[i].closepx
                            , klineData[i].closepx
                            , klineData[i].closepx
                            , klineData[i++].closepx));
                        }
                    }

                    //防止死循环
                    if(timeCursor.TimeOfDay == new TimeSpan(23,59,0))
                    {
                        break;
                    }
                }
            }

            return klineData;
        }

        /// <summary>
        /// 合并一种每个月每种种类的数据到一个文件之中，主力合约切换时日盘数据取切换前一个合约的日盘，夜盘数据取切换后合约的夜盘数据
        /// 另外，此函数是根据AppConfig.MAIN_CONTRACT_INFO_OUTPUT_FIX_PATH中的修正后主力合约信息找出主力合约切换日期的
        /// </summary>
        public static void FinalProcess()
        {
            //读取主力合约信息到mainContracts中
            List<MainContract> mainContracts = new List<MainContract>();
            FileStream fs_info = new FileStream(AppConfig.MAIN_CONTRACT_INFO_OUTPUT_FIX_PATH, FileMode.Open);
            StreamReader sr_info = new StreamReader(fs_info, Encoding.UTF8);
            string line_info = null;
            while((line_info=sr_info.ReadLine())!=null)
            {
                string[] list = line_info.Split(',');
                mainContracts.Add(new MainContract(
                    list[0]
                    , Convert.ToDateTime(list[1])
                    , list[2]
                    , list[3]
                    , new FileInfo(AppConfig.DATA_OUTPUT_ROOT_DIR + Convert.ToDateTime(list[1]).ToString("yyyyMM") + "\\" + list[0] + "\\" + list[4])
                    ,0));
            }
            fs_info.Dispose();
            sr_info.Dispose();

            //根据mainContracts中的信息，合并每个与每种种类的主力合约
            foreach (var yearMC in mainContracts.GroupBy(item=>item.date.Year))
            {
                foreach(var monthMC in yearMC.GroupBy(item=>item.date.Month))
                {
                    foreach(var type in monthMC.GroupBy(item=>item.type))
                    {
                        string contents = "";
                        foreach (var g in type.GroupBy(item=>item.date.Date))
                        {
                            string content = "";
                            FileStream fs = null;
                            StreamReader sr = null;
                            if(g.Count() == 2)//有两项的为切换日，在夜盘正式切换
                            {
                                //选取切换日盘的数据
                                fs = g.First().file.OpenRead();
                                sr = new StreamReader(fs, Encoding.UTF8);
                                string line = null;
                                while((line=sr.ReadLine())!=null)
                                {
                                    string[] list = line.Split(',');
                                    if(Convert.ToDateTime(list[2]).TimeOfDay>new TimeSpan(15,0,0))
                                    {
                                        break;
                                    }
                                    content += line + "\r\n";
                                }
                                fs.Close();
                                sr.Close();
                                line = null;
                                //选取切换后合约的夜盘数据
                                fs = g.Last().file.OpenRead();
                                sr = new StreamReader(fs, Encoding.UTF8);
                                while ((line = sr.ReadLine()) != null)
                                {
                                    string[] list = line.Split(',');
                                    if (Convert.ToDateTime(list[2]).TimeOfDay < new TimeSpan(15, 0, 0))
                                    {
                                        continue ;
                                    }
                                    content += line + "\r\n";
                                }
                                contents += content;
                            }
                            else
                            {
                                //合约未切换，直接合并该文件的数据
                                fs = g.First().file.OpenRead();
                                sr = new StreamReader(fs, Encoding.UTF8);
                                contents +=sr.ReadToEnd();
                            }
                            fs.Dispose();
                            sr.Dispose();
                        }
                        string dirPath = AppConfig.DATA_FINAL_OUTPUT_ROOT_DIR + type.First().date.ToString("yyyyMM") + "\\" + type.First().type + "\\";
                        if(!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }
                        File.WriteAllText(dirPath + type.First().type + type.First().date.ToString("yyyyMM") + ".csv", contents);
                        Console.WriteLine("合并完成：" + dirPath + type.First().type + type.First().date.ToString("yyyyMM") + ".csv");
                    }
                }

            }
        }

        /// <summary>
        /// 判断某一时间是否在交易时间内
        /// </summary>
        /// <param name="datetime">需要进行判断的时间</param>
        /// <returns>在交易时间内的返回true，否则返回false</returns>
        private static bool isInTransactionHour(DateTime datetime)
        {
            bool flag = false;
            foreach(var th in KLineConbine.TransactionHour)
            {
                if(datetime.TimeOfDay>=th.Key.TimeOfDay && datetime.TimeOfDay<th.Value.TimeOfDay)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 判断某一时间是否是每一段交易时间的最后时刻
        /// </summary>
        /// <param name="datetime">需要进行判断的时间</param>
        /// <returns>是最后时刻返回true，否则返回false</returns>
        private static bool isInLastTransactionHour(DateTime datetime)
        {
            bool flag = false;
            foreach (var th in KLineConbine.TransactionHour)
            {
                if (datetime.TimeOfDay == th.Value.TimeOfDay)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }
}
