using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLine_CTA_N_MIN
{
    class KLineDataUnit
    {
        string contractID;
        string contractName;
        DateTime tdatetime;
        double highpx;
        double lowpx;
        double openpx;
        double closepx;
        public long group;

        public KLineDataUnit(string cid, string cname, DateTime dt, double hpx, double lpx, double opx, double cpx, long g)
        {
            contractID = cid;
            contractName = cname;
            tdatetime = dt;
            highpx = hpx;
            lowpx = lpx;
            openpx = opx;
            closepx = cpx;
            group = g;
        }

        public string ContractID
        {
            get
            {
                return contractID;
            }

            set
            {
                contractID = value;
            }
        }

        public string ContractName
        {
            get
            {
                return contractName;
            }

            set
            {
                contractName = value;
            }
        }

        public DateTime Tdatetime
        {
            get
            {
                return tdatetime;
            }

            set
            {
                tdatetime = value;
            }
        }

        public double Highpx
        {
            get
            {
                return highpx;
            }

            set
            {
                highpx = value;
            }
        }

        public double Lowpx
        {
            get
            {
                return lowpx;
            }

            set
            {
                lowpx = value;
            }
        }

        public double Openpx
        {
            get
            {
                return openpx;
            }

            set
            {
                openpx = value;
            }
        }

        public double Closepx
        {
            get
            {
                return closepx;
            }

            set
            {
                closepx = value;
            }
        }
    }
}
