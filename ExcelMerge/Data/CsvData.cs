using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge.Data
{
    public class CsvData
    {
        public CsvFrame this[string id]
        {
            get
            {
                //Console.WriteLine("2:" + (DateTime.Now - time).Ticks);
                //var time = DateTime.Now;
                //return Frames.Single(x => x.Id == id);
                return FrameIdDict[id];
            }
        }
        private List<string> TextColumnList { get; set; }
        private string IdColumn { get; set; }
        public List<CsvFrame> Frames { get; set; }
        public List<string> Columns { get; set; }
        private List<List<string>> ColumnsTable { get; set; }
        public string FileName { get; set; }
        private static ILog log = LogManager.GetLogger("CsvData");
        private int HeaderRows { get; set; }
        private Dictionary<string,CsvFrame> FrameIdDict { get; set; }

        public CsvData() 
        {
            Frames = new List<CsvFrame>();
            Columns = new List<string>();
            ColumnsTable = new List<List<string>>();
            FrameIdDict = new Dictionary<string, CsvFrame>();
        }

        public CsvData(string filePath, string id, List<string> text_column, int headerRows) :this()
        {
            IdColumn = id;
            TextColumnList = text_column;
            FileName = Path.GetFileName(filePath);
            HeaderRows = headerRows;
            var data = ReadFile(filePath);
            LoadData(data);
        }

        private string ReadFile(string filePath)
        {
            var res = "";
            log.Info("载入文件：" + filePath);
            using (var rd = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                res += rd.ReadToEnd();
            }
            return res;
        }

        private void LoadData(string data)
        {
            data = Escape(data);
            var row_array = data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i<row_array.Length; i++)
            {
                if (i < HeaderRows)
                {
                    ColumnsTable.Add(row_array[i].Split(',').Select(j => j.Trim()).ToList());
                } 
                else {
                    if (i == HeaderRows) InitColumn();
                    try
                    {
                        var frame = new CsvFrame(row_array[i], Columns, IdColumn, TextColumnList);
                        FrameIdDict.Add(frame.Id, frame);
                        Frames.Add(frame);
                    } catch(Exception e)
                    {
                        log.Error("行分析错误,将跳过该行分析：" + row_array[i], e);
                    }
                }
            }
        }

        private void InitColumn()
        {
            var calcArray = ColumnsTable.Select(i => i[0]).ToList();
            Columns.Add(GetHeaderString(ColumnsTable.Select(i => i[0]).ToList()));
            for(int i = 1; i < ColumnsTable[0].Count(); i++) 
            {
                for(int j = 0; j < ColumnsTable.Count(); j++)
                {
                    if (ColumnsTable[j][i] != "" && ColumnsTable[j][i] != calcArray[j])
                    {
                        calcArray[j] = ColumnsTable[j][i];
                        for(var k = j + 1; k < calcArray.Count(); k++)
                        {
                            calcArray[k] = "";
                        }
                    }
                }
                Columns.Add(GetHeaderString(calcArray));
            }
            log.Info("压缩后的列名：【" + String.Join(",", Columns) + "】");
        }

        private string GetHeaderString(List<string> list)
        {
            return String.Join("/", list.Where(i => i != ""));
        }

        private string Escape(string str)
        {
            var res = "";
            var strList = str.Split('"');
            for(var i=0;i<strList.Length;i++)
            {
                if(i%2 == 1)
                {
                    if (strList[i].Contains(','))
                    {
                        res += strList[i].Replace(",", "，");
                    } else
                    {
                        res += "“" + strList[i] + "”";
                    }

                }
                else
                {
                    res += strList[i];
                }
            }
            return res;
        }
    }
}
