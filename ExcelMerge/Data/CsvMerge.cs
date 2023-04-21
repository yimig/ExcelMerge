using log4net;
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
        private static ILog log = LogManager.GetLogger("Merge");
        public static string BeginMerge(List<CsvData> data,ReferenceMode refMode,ReferenceSetting setting) 
        {
            CsvData refCsv = new CsvData();
            log.Info("当前引用文件指定模式：" + refMode.ToString()+"，顺序索引："+setting.IsFirstFile.ToString());
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
            log.Info("当前内容匹配模式：" + setting.ReferenceType);
            if(setting.ReferenceType == "date")
            {
                refCsv = GetDateSortReference(data, setting);
            }
            else
            {
                refCsv = GetIntegerSortReference(data, setting);
            }
            

            return refCsv;
        }

        private static CsvData GetReferenceFileWithFileNameMode(List<CsvData> data, ReferenceSetting setting)
        {
            CsvData refCsv;
            try
            {
                var pathList = new List<string>();
                Directory.GetFiles(setting.FileSource).Where(i => Path.GetExtension(i) == ".csv").ToList().ForEach(i => pathList.Add(Path.GetFileName(i)));
                log.Info("引用文件分析字典创建成功");
                pathList.Sort();
                refCsv = data.Single(i => i.FileName == (setting.IsFirstFile ? pathList.First() : pathList.Last()));
                log.Info("确定引用文件：" + refCsv.FileName);
                data.Remove(refCsv);
            } catch (Exception ex)
            {
                log.Error("分析引用文件失败,将改用指定文件名分析" + ex);
                refCsv = GetReferenceFileWithStaticFileMode(data, setting);
            }
            return refCsv;
        }

        private static CsvData GetReferenceFileWithCreateDateMode(List<CsvData> data, ReferenceSetting setting)
        {
            CsvData refCsv;
            try
            {
                var dateList = new List<DateTime>();
                Directory.GetFiles(setting.FileSource).Where(i => Path.GetExtension(i) == ".csv").ToList().ForEach(i => dateList.Add(new FileInfo(i).CreationTime));
                log.Info("引用文件分析字典创建成功");
                dateList.Sort();
                refCsv = data.Single(i => new FileInfo(setting.FileSource + i.FileName).CreationTime == (setting.IsFirstFile ? dateList.First() : dateList.Last()));
                log.Info("确定引用文件：" + refCsv.FileName);
                data.Remove(refCsv);
            }catch(Exception ex)
            {
                log.Error("分析引用文件失败,将改用文件名排序分析" + ex);
                refCsv = GetReferenceFileWithFileNameMode(data, setting);
            }

            return refCsv;
        }

        private static CsvData GetReferenceFileWithStaticFileMode(List<CsvData> data, ReferenceSetting setting)
        {
            CsvData refCsv;
            try
            {
                refCsv = data.Single(i => i.FileName == setting.ReferenceFileName);
                log.Info("使用指定的文件作为引用文件：" + setting.ReferenceFileName);
            } catch (Exception ex)
            {
                log.Error("确定指定文件失败，文件名【"+setting.ReferenceFileName+"】，将使用默认第一个文件作为引用文件",ex);
                refCsv = data[0];
                log.Info("使用引用文件：" + refCsv.FileName);
            }
            data.Remove(refCsv);
            return refCsv;

        }

        private static CsvData GetIntegerSortReference(List<CsvData> data, ReferenceSetting setting)
        {
            try
            {
                var dict = data.ToDictionary(i => Convert.ToInt32(i.Frames[0].TextDict[setting.ReferenceColumn]));
                log.Info("引用文件分析字典创建成功");
                int max_index = setting.IsFirstFile ? dict.Keys.Max() : dict.Keys.Last();
                log.Info("确定引用文件：" + dict[max_index].FileName);
                data.Remove(dict[max_index]);
                return dict[max_index];
            } catch (Exception ex)
            {
                log.Error("分析引用文件失败,将改用文件名排序分析" + ex);
                return GetReferenceFileWithFileNameMode(data, setting);
            }

        }

        private static CsvData GetDateSortReference(List<CsvData> data, ReferenceSetting setting)
        {
            log.Info("载入日期表达式：" + setting.DatePattern);
            var datetimeInfo = new System.Globalization.DateTimeFormatInfo();
            datetimeInfo.ShortDatePattern = setting.DatePattern;
            try
            {
                var dict = data.ToDictionary(i => Convert.ToDateTime(i.Frames[0].TextDict[setting.ReferenceColumn], datetimeInfo));
                log.Info("引用文件分析字典创建成功");
                var max_index = setting.IsFirstFile ? dict.Keys.OrderBy(i => i.Month).First() : dict.Keys.OrderBy(i => i.Month).Last();
                log.Info("确定引用文件：" + dict[max_index].FileName);
                data.Remove(dict[max_index]);
                return dict[max_index];
            } catch (Exception ex)
            {
                log.Error("分析引用文件失败,将改用文件名排序分析" + ex);
                return GetReferenceFileWithFileNameMode(data, setting);
            }

        }

        private static Dictionary<string,List<string>> MergeCsv(List<CsvData> datas,CsvData refData)
        {
            var resDict = new Dictionary<string, List<string>>();
            refData.Columns.ForEach(i => resDict.Add(i, new List<string>()));
            refData.Frames.ForEach(f => f.TextDict.Keys.ToList().ForEach(i => resDict[i].Add(f.TextDict[i])));
            int index = 0;
            foreach (var refFrame in refData.Frames)
            {
                foreach(var column in refFrame.DecimalDict.Keys)
                {
                    try
                    {
                        var value = refFrame.DecimalDict[column];
                        foreach (var data in datas)
                        {
                            try
                            {
                                value += data[refFrame.Id].DecimalDict[column];
                            } catch(Exception ex)
                            {
                                log.Error(data.FileName + "中的【" + column + "】列，ID=" + refFrame.Id + "， 无法找到，将不参与运算！",ex);
                            }
                        }
                        resDict[column].Add(value.ToString());
                    } catch (Exception ex)
                    {
                        log.Error("ID="+refFrame.Id+"，列【" + column + "】分析失败,结果中可能丢失该值！", ex);
                        resDict[column].Add("");
                    }

                }
                Console.WriteLine("进度：【" + (++index)+"/"+refData.Frames.Count()+"】");
            }
            return resDict;
        }

        private static string ConvertCsvString(Dictionary<string, List<string>> dict)
        {
            log.Info("将结果转化为CSV内容");
            var res = new StringBuilder();
            res.Append(String.Join(",",dict.Keys) + "\n");
            var lastKey = dict.Keys.Last();
            for(var i = 0; i < dict[dict.Keys.First()].Count();i++)
            {
                Console.WriteLine("Converting: 【" + i + "/" + dict[dict.Keys.First()].Count() + "】");
                foreach (var key in dict.Keys)
                {
                    try
                    {
                        if (key != lastKey)
                        {
                            res.Append(dict[key][i] + ",");
                        }
                        else
                        {
                            res.Append(dict[key][i] + "\n");
                        }
                    } catch (Exception ex)
                    {
                        log.Info("在字典中找不到key=" + key + ",index=" + i + "的项，文件可能损坏");
                        if (key != lastKey)
                        {
                            res.Append(",");
                        }
                        else
                        {
                            res.Append("\n");
                        }
                    }

                }
            }
            return res.Remove(res.Length-1,1).ToString();
        }

    }
}
