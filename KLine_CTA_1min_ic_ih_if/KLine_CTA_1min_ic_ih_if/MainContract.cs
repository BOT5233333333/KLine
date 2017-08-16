using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KLine_CTA_1min_ic_ih_if
{
    class MainContract
    {
        /// <summary>
        /// 保存需要处理的主力合约种类
        /// </summary>
        private static List<string> Types = new List<string>();
        /// <summary>
        /// 保存主力合约信息
        /// </summary>
        private static List<MainContract> MainContracts = new List<MainContract>();

        /// <summary>
        /// 主函数，先从文件获取需要找的主力合约类型，再执行查找操作，最后输出主力合约信息到一个csv文件中
        /// </summary>
        public static void Find()
        {
            Types = GetTypesFromTxtFile();
            MainContracts = FindMainContract();
            PrintMainContractsInfo();
        }

        /// <summary>
        /// 输出主力合约信息到AppConfig.MAIN_CONTRACT_INFO_OUTPUT_PATH的csv文件中
        /// </summary>
        private static void PrintMainContractsInfo()
        {
            List<string> contents = new List<string>();
            foreach(var mc in MainContract.MainContracts.OrderBy(item=>item.type).ThenBy(item=>item.date))
            {
                contents.Add(string.Format("{0},{1},{2},{3},{4}", mc.type, mc.date, mc.contractID, mc.contractName, mc.file.Name));
            }
            File.WriteAllLines(AppConfig.MAIN_CONTRACT_INFO_OUTPUT_PATH, contents);
        }

        /// <summary>
        /// 根据AppConfig.MAIN_CONTRACT_TYPES_INFO_PATH文件获取主力合约种类到类静态变量中
        /// </summary>
        /// <returns>主力合约种类，每个字符串为一种主力合约种类</returns>
        private static List<string> GetTypesFromTxtFile()
        {
            List<string> types = new List<string>();

            FileStream fs = new FileStream(AppConfig.MAIN_CONTRACT_TYPES_INFO_PATH, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            string line = null;
            while((line=sr.ReadLine())!=null)
            {
                types.Add(line);
            }
            fs.Dispose();
            sr.Dispose();

            return types;
        }


        public string type;
        public DateTime date;
        public string contractID;
        public string contractName;
        public FileInfo file;
        uint tq;

        public MainContract(string t, DateTime d, string cID, string cName, FileInfo f, uint tq)
        {
            this.type = t;
            this.date = d;
            this.contractID = cID;
            this.contractName = cName;
            this.file = f;
            this.tq = tq;
        }

        /// <summary>
        /// 在源数据文件中找到主力合约并返回，需要找的合约根据静态变量Types决定。
        /// 一天中每种类型的合约有一个主力合约。
        /// 某种类型的主力合约即这一天中该类型合约日盘最终时刻（15:00）的累计成交量最大的合约。
        /// </summary>
        /// <returns>主力合约信息</returns>
        static private List<MainContract> FindMainContract()
        {
            List<MainContract> mainContracts = new List<MainContract>();

            DirectoryInfo root = new DirectoryInfo(AppConfig.DATA_SOURCE_ROOT_DIR);

            foreach(var monthDir in root.GetDirectories())
            {
                foreach (var dir in monthDir.GetDirectories().Where(d =>  (d.Name == "if" || d.Name == "ic" || d.Name == "ih") && d.GetFiles().Length > 0))
                {

                    List<MainContract> infos = new List<MainContract>();
                    //将一个文件夹中的所有信息都放进infos中待处理

                    foreach (var file in dir.GetFiles())
                    {
                        FileStream fs = file.OpenRead();
                        StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                        string line = null;
                        UInt32 tq = 0;
                        string cName = null;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] list = line.Split(',');
                            try
                            {
                                DateTime dt = new DateTime(int.Parse(list[1].Substring(0, 4)), int.Parse(list[1].Substring(4, 2)), int.Parse(list[1].Substring(6, 2)), int.Parse(list[2].Substring(0, 2)), int.Parse(list[2].Substring(2, 2)), int.Parse(list[2].Substring(4, 2)), int.Parse(list[3]));
                                if (dt.TimeOfDay < new TimeSpan(15, 15, 0))
                                {
                                    tq = Convert.ToUInt32(list[39]);
                                    cName = list[0];
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch
                            {

                            }
                        }

                        infos.Add(new MainContract(
                        dir.Name
                        , new DateTime(int.Parse(file.Name.Substring(5 + dir.Name.Length, 4)), int.Parse(file.Name.Substring(9 + dir.Name.Length, 2)), int.Parse(file.Name.Substring(11 + dir.Name.Length, 2)))
                        , file.Name.Substring(0, 4 + dir.Name.Length)
                        , cName
                        , file
                        , tq));

                        fs.Close();
                        sr.Close();
                    }

                    //根据日期进行分组，每一组中是同一天内的不同合约
                    foreach (var g in infos.GroupBy(item => item.date))
                    {
                        mainContracts.Add(g.OrderByDescending(item => item.tq).First());
                    }
                    Console.WriteLine("找到主力合约，" + monthDir.Name + "\\" + dir.Name);

                }
            }

            return mainContracts;
        }


    }
}
