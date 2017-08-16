using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLine_CTA_1min_ic_ih_if
{
    class AppConfig
    {
        public const string MAIN_CONTRACT_TYPES_INFO_PATH = @"G:\KLine\主力合约种类.txt";
        public const string DATA_SOURCE_ROOT_DIR = @"G:\CTA\2015\";
        public const string DATA_OUTPUT_ROOT_DIR = @"G:\KLine\CTA_OUTPUT\";
        public const string DATA_FINAL_OUTPUT_ROOT_DIR = @"G:\KLine\CTA_OUTPUT_FINAL\";
        public const string MAIN_CONTRACT_INFO_OUTPUT_PATH = @"G:\KLine\主力合约.csv";
        public const string MAIN_CONTRACT_INFO_OUTPUT_FIX_PATH = @"G:\KLine\主力合约_修正.csv";
        public const string CTA_MAIN_CONTRACT_TEMP_DIR = @"G:\KLine\CTA_MAIN_CONTRACT_TEMP\";

    }
}
