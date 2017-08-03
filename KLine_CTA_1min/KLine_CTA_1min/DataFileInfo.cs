using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLine_CTA_1min
{
    class DataFileInfo
    {
        string type;
        DateTime date;
        string contractName;
        string fileName;

        public DataFileInfo(string t, DateTime d, string cName, string fName)
        {
            this.type = t;
            this.date = d;
            this.contractName = cName;
            this.fileName = fName;
        }
    }
}
