using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge.Data
{
    public static class CsvMerge
    {

        public static string BeginMerge(List<CsvData> data,ReferenceMode refMode,ReferenceSetting setting) 
        {
            CsvData refCsv = new CsvData();
            switch (refMode)
            {
                case ReferenceMode.Content:
                    {
                        refCsv = GetReferenceFileWithContentMode(data, setting);
                        break;
                    }
                case ReferenceMode.CreateDate:
                    {
                        refCsv = GetReferenceFileWithCreateDateMode(data, setting);
                        break;
                    }
                case ReferenceMode.FileName:
                    {
                        refCsv = GetReferenceFileWithFileNameMode(data, setting);
                        break;
                    }
                case ReferenceMode.Static:
                    {
                        refCsv = GetReferenceFileWithStaticFileMode(data, setting);
                        break;
                    }
            }
            var resDict = MergeCsv(data, refCsv);
            return ConvertCsvString(resDict);
        }

        private static CsvData GetReferenceFileWithContentMode(List<CsvData> data,ReferenceSetting setting)
        {
            CsvData refCsv;
            if(setting.ReferenceType == "date")
            {
                refCsv = GetDateSortReference(data, setting.ReferenceColumn, setting.DatePattern, setting.IsFirstFile);
            }
            else
            {
                refCsv = GetIntegerSortReference(data, setting.ReferenceColumn, setting.IsFirstFile);
            }
            

            return refCsv;
        }

        private static CsvData GetReferenceFileWithFileNameMode(List<CsvData> data, ReferenceSetting setting)
        {
            CsvData refCsv;
            var pathList = new List<string>();
            Directory.GetFiles(setting.FileSource).Where(i => Path.GetExtension(i) == ".csv").ToList().ForEach(i=>pathList.Add(Path.GetFileName(i)));
            pathList.Sort();
            refCsv = data.Single(i=> i.FileName == (setting.IsFirstFile ? pathList.First():pathList.Last()));
            data.Remove(refCsv);
            return refCsv;
        }

        private static CsvData GetReferenceFileWithCreateDateMode(List<CsvData> data, ReferenceSetting setting)
        {
            CsvData refCsv;
            var dateList = new List<DateTime>();
            Directory.GetFiles(setting.FileSource).Where(i => Path.GetExtension(i) == ".csv").ToList().ForEach(i => dateList.Add(new FileInfo(i).CreationTime));
            dateList.Sort();
            refCsv = data.Single(i => new FileInfo(setting.FileSource + i.FileName).CreationTime == (setting.IsFirstFile ? dateList.First() : dateList.Last()));
            data.Remove(refCsv);
            return refCsv;
        }

        private static CsvData GetReferenceFileWithStaticFileMode(List<CsvData> data, ReferenceSetting setting)
        {
            CsvData refCsv = data.Single(i => i.FileName == setting.ReferenceFileName);
            data.Remove(refCsv);
            return refCsv;
        }

        private static CsvData GetIntegerSortReference(List<CsvData> data, string column, bool isFirst)
        {
            var dict = data.ToDictionary(i => Convert.ToInt32(i.Frames[0].TextDict[column]));
            int max_index = isFirst? dict.Keys.Max() : dict.Keys.Last();
            data.Remove(dict[max_index]);
            return dict[max_index];
        }

        private static CsvData GetDateSortReference(List<CsvData> data, string column, string pattern, bool isFirst)
        {
            var datetimeInfo = new System.Globalization.DateTimeFormatInfo();
            datetimeInfo.ShortDatePattern = pattern;
            var dict = data.ToDictionary(i => Convert.ToDateTime(i.Frames[0].TextDict[column], datetimeInfo));
            var max_index = isFirst? dict.Keys.OrderBy(i => i.Month).First(): dict.Keys.OrderBy(i => i.Month).Last();
            data.Remove(dict[max_index]);
            return dict[max_index];
        }

        private static Dictionary<string,List<string>> MergeCsv(List<CsvData> datas,CsvData refData)
        {
            var resDict = new Dictionary<string, List<string>>();
            refData.Columns.ForEach(i => resDict.Add(i, new List<string>()));
            refData.Frames.ForEach(f => f.TextDict.Keys.ToList().ForEach(i => resDict[i].Add(f.TextDict[i])));
            foreach (var refFrame in refData.Frames)
            {
                foreach(var column in refFrame.DecimalDict.Keys)
                {
                    var value = refFrame.DecimalDict[column];
                    foreach(var data in datas)
                    {
                        value += data[refFrame.Id].DecimalDict[column];
                    }
                    resDict[column].Add(value.ToString());
                }
            }
            return resDict;
        }

        private static string ConvertCsvString(Dictionary<string, List<string>> dict)
        {
            var res = String.Join(",",dict.Keys) + "\n";
            var lastKey = dict.Keys.Last();
            for(var i = 0; i < dict[dict.Keys.First()].Count();i++)
            {
                foreach (var key in dict.Keys)
                {
                    if(key!=lastKey)
                    {
                        res += dict[key][i] + ",";
                    }
                    else
                    {
                        res += dict[key][i] + "\n";
                    }
                }
            }
            return res.Remove(res.Length - 1);
        }

    }
}
