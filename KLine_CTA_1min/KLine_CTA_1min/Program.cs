using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLine_CTA_1min
{
    class Program
    {
        static void Main(string[] args)
        {
            //MainContract.Find();
            //执行Find()后需要手动修改输出的主力合约信息csv文件，因为其中有数据缺失的文件会导致信息表错误，需要手动修正使每天的主力合约正确
            KLineConbine.FixMainContractInfo();
            KLineConbine.Conbine_1MIin();
            KLineConbine.FinalProcess();
        }
    }
}
