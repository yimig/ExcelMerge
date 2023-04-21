using ExcelMerge.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge
{
    internal class Program
    {
        private static Dictionary<string, ReferenceMode> MODE_DICT = new Dictionary<string, ReferenceMode>()
        {
            {"content", ReferenceMode.Content },
            {"name", ReferenceMode.FileName },
            {"create", ReferenceMode.CreateDate },
            {"static", ReferenceMode.Static }
        };
        private static ReferenceSetting SETTING = new ReferenceSetting()
        {
                ReferenceColumn = ConfigurationManager.AppSettings["ref_column"],
                ReferenceType = ConfigurationManager.AppSettings["ref_type"],
                DatePattern = ConfigurationManager.AppSettings["date_pattern"],
                IsFirstFile = ConfigurationManager.AppSettings["is_first"] == "true",
                ReferenceFileName = ConfigurationManager.AppSettings["ref_file"],
                FileSource = ConfigurationManager.AppSettings["source"]
        };
        private static ILog log = LogManager.GetLogger("Main");

        static void Main(string[] args)
        {
            log.Info("分析开始");
            var dataList = new List<CsvData>();
            var id = ConfigurationManager.AppSettings["id_key"];
            var text_list = ConfigurationManager.AppSettings["text_column"] == "" ? new List<string>() : ConfigurationManager.AppSettings["text_column"].Split(',').ToList();
            var header_rows = Convert.ToInt32(ConfigurationManager.AppSettings["header_count"]);
            Directory.GetFiles(ConfigurationManager.AppSettings["source"]).Where(i => Path.GetExtension(i) == ".csv").ToList().ForEach(file => dataList.Add(new CsvData(file, id, text_list, header_rows)));
            log.Info("共载入了【" + dataList.Count() + "】个文件");
            if(dataList.Count>1)
            {
                try
                {
                    var resStr = CsvMerge.BeginMerge(dataList, MODE_DICT[ConfigurationManager.AppSettings["merge_mode"]], SETTING);
                    WriteFile(ConfigurationManager.AppSettings["result"], resStr);
                } catch(Exception ex)
                {
                    log.Error("合并文件错误", ex);
                }

            }
            else
            {
                log.Info("可分析文件数少于2个，不需要合并，程序退出。");
            }
            log.Info("所有工作已经完成,按任意键退出");
            Console.ReadLine();
        }

        static void WriteFile(string filePath, string content)
        {
            log.Info("导出文件：" + filePath);
            using (var wt = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8))
            {
                wt.Write(content);
            }
        }
    }
}
