using ExcelMerge.Data;
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

        static void Main(string[] args)
        {
            var dataList = new List<CsvData>();
            var id = ConfigurationManager.AppSettings["id_key"];
            var text_list = ConfigurationManager.AppSettings["text_column"] == "" ? new List<string>() : ConfigurationManager.AppSettings["text_column"].Split(',').ToList();
            Directory.GetFiles(ConfigurationManager.AppSettings["source"]).Where(i => Path.GetExtension(i) == ".csv").ToList().ForEach(file => dataList.Add(new CsvData(file, id, text_list)));
            var setting = new ReferenceSetting()
            {
                ReferenceColumn = ConfigurationManager.AppSettings["ref_column"],
                ReferenceType = ConfigurationManager.AppSettings["ref_type"],
                DatePattern = ConfigurationManager.AppSettings["date_pattern"],
                IsFirstFile = ConfigurationManager.AppSettings["is_first"] == "true",
                ReferenceFileName = ConfigurationManager.AppSettings["text_column"],
                FileSource = ConfigurationManager.AppSettings["source"]
            };
            var resStr = CsvMerge.BeginMerge(dataList, MODE_DICT[ConfigurationManager.AppSettings["merge_mode"]], setting);
            WriteFile(ConfigurationManager.AppSettings["result"],resStr);
        }

        static void WriteFile(string filePath, string content)
        {
            using (var wt = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8))
            {
                wt.Write(content);
            }
        }
    }
}
