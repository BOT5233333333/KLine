using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KLineDbStore_Load
{
    class SPTxtToSqlClass
    {
        private string rootPath;
        private string dbName;
        private DBConnection conn;

        private int numAllFiles;
        private static int numFilesFinish;

        private string sqlOutPutPath = @"G:\KLine\KLine_sql_load2015.txt";

        //private string connStr = "Server=192.168.2.134;User ID=root;Password=123456;Database=KLineTest;CharSet=utf8";
        private string connStr = "Server=192.168.2.181;User ID=root;Password=123456;Database=CTAHisDBSPFT_K;CharSet=utf8";
        public SPTxtToSqlClass(string rootPath, int numAllFiles)
        {
            this.rootPath = rootPath;
            this.dbName = "CTAHisDBSPFT_K";
            this.conn = new DBConnection(connStr);
            numFilesFinish = 0;
            this.numAllFiles = numAllFiles;
        }

        internal void MainFunc()
        {
            List<string> querys = new List<string>();
            string query = "";

            DirectoryInfo rootDir = new DirectoryInfo(this.rootPath);


            foreach (var monthDir in rootDir.GetDirectories())
            {
                List<Task<int>> tasks = new List<Task<int>>();
                Console.WriteLine("*************");
                Console.WriteLine("文件夹:" + monthDir.FullName + "开始");
                Console.WriteLine("时间:" + DateTime.Now);
                Console.WriteLine("*************");
                foreach (var dir in monthDir.GetDirectories())
                {
                    foreach (var file in dir.GetFiles())
                    {
                        string tableName = "CTA_K_1MIN_" + file.Name.Replace(".csv", "").ToUpper() + "_TBL";
                        TableProcess(tableName, this.dbName);
                        query = string.Format("LOAD DATA LOCAL INFILE '{0}' REPLACE INTO TABLE {1} FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"' LINES TERMINATED BY '\\r\\n' "
                            + "(contractid,contractname,tdatetime,highpx,lowpx,openpx,closepx);"
                               , file.FullName.Replace(@"\", @"\\")
                               , tableName);
                        querys.Add(query);
                        Console.WriteLine(string.Format("{0}/{1}", ++numFilesFinish, numAllFiles));
                    }
                }
                //@dummy
                Console.WriteLine("*************");
                Console.WriteLine("文件夹:" + monthDir.FullName + "结束");
                Console.WriteLine("时间:" + DateTime.Now);
                Console.WriteLine("*************");

                File.WriteAllLines(this.sqlOutPutPath, querys);
            }
        }

        private void TableProcess(string tableName, string dbName)
        {
            string SqlQryTbl = "select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA =" + "\"" + dbName + "\""
                    + " and TABLE_NAME = " + "\"" + tableName + "\"";
            Object res = this.conn.ExecuteScalar(SqlQryTbl);
            string SqlCreateTbl = "";
            if (res == null)
            {
                Console.WriteLine("创建表:" + tableName);
                SqlCreateTbl = "CREATE TABLE " + tableName + "(recordID INT NOT NULL auto_increment,contractid CHAR(6),contractname CHAR(15),"
                            + "tdatetime DATETIME,highpx DOUBLE,lowpx DOUBLE,openpx DOUBLE,closepx DOUBLE,"
                            + "PRIMARY KEY (recordID)) ENGINE=InnoDB DEFAULT CHARSET=utf8;";
            }

            this.conn.ExecuteNonQuery(SqlCreateTbl);
        }
    }


}
