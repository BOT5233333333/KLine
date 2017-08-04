using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KLine_CTA_1min
{
    class MainContract
    {
        private static List<string> Types = new List<string>();
        private static List<MainContract> MainContracts = new List<MainContract>();
        public static void Find()
        {
            Types = GetTypesFromTxtFile();
            MainContracts = FindMainContract();
            PrintMainContractsInfo();
        }

        private static void PrintMainContractsInfo()
        {
            List<string> contents = new List<string>();
            foreach(var mc in MainContract.MainContracts)
            {
                contents.Add(string.Format("{0},{1},{2},{3},{4}", mc.type, mc.date, mc.contractID, mc.contractName, mc.file.Name));
            }
            File.WriteAllLines(AppConfig.MAIN_CONTRACT_INFO_OUTPUT_PATH, contents);
        }

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


        string type;
        DateTime date;
        string contractID;
        string contractName;
        FileInfo file;
        uint openints;

        public MainContract(string t, DateTime d, string cID, string cName, FileInfo f, uint op)
        {
            this.type = t;
            this.date = d;
            this.contractID = cID;
            this.contractName = cName;
            this.file = f;
            this.openints = op;
        }

        static private List<MainContract> FindMainContract()
        {
            List<MainContract> mainContracts = new List<MainContract>();

            DirectoryInfo root = new DirectoryInfo(AppConfig.DATA_SOURCE_ROOT_DIR);
            foreach(var monthDir in root.GetDirectories())
            {
                foreach(var dir in monthDir.GetDirectories().Where(d=>d.Name!="t" && d.Name != "tf" && d.Name != "if" && d.Name != "ic" && d.Name != "ih" && d.GetFiles().Length>0))
                {
                    FileInfo[] files = dir.GetFiles();
                    FileStream fs = files[0].OpenRead();
                    StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                    string[] trade = sr.ReadLine().Split(',');
                    //如果此文件夹中第一个文件的包含了这个实例要找出的主力合约的类型的字段，则在这个文件夹中进行处理
                    if( MainContract.Types.Exists(t=> trade[1].Contains(t)) )
                    {
                        List<MainContract> infos = new List<MainContract>();
                        //将一个文件夹中的所有信息都放进infos中待处理
                        foreach (var file in files)
                        {
                            fs = file.OpenRead();
                            sr = new StreamReader(fs, Encoding.UTF8);
                            string[] line = sr.ReadLine().Split(',');
                            uint curOpenints = uint.Parse(line[10]);
                            string cName = line[1];

                            infos.Add(new MainContract(
                                dir.Name
                                , new DateTime(int.Parse(file.Name.Substring(5 + dir.Name.Length, 4)), int.Parse(file.Name.Substring(9 + dir.Name.Length, 2)), int.Parse(file.Name.Substring(11 + dir.Name.Length, 2)))
                                , file.Name.Substring(0, 4 + dir.Name.Length)
                                , cName
                                , file
                                , curOpenints));

                            fs.Close();
                            sr.Close();
                        }

                        //根据日期进行分组，每一组中是同一天内的不同合约
                       foreach(var g in infos.GroupBy(item => item.date))
                        {
                            mainContracts.Add(g.OrderByDescending(item=>item.openints).First());
                        }
                        Console.WriteLine("找到主力合约，" + monthDir.Name + "\\" + dir.Name);
                    }
                    else
                    {
                        fs.Close();
                        sr.Close();
                    }

                }
            }

            return mainContracts;
        }


    }
}
