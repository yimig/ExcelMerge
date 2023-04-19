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
                return Frames.Single(x => x.Id == id);
            }
        }
        private List<string> TextColumnList { get; set; }
        private string IdColumn { get; set; }
        public List<CsvFrame> Frames { get; set; }
        public List<string> Columns { get; set; }
        public string FileName { get; set; }
        public CsvData() 
        {
            Frames = new List<CsvFrame>();
            Columns = new List<string>();
        }

        public CsvData(string filePath, string id, List<string> text_column) :this()
        {
            IdColumn = id;
            TextColumnList = text_column;
            FileName = Path.GetFileName(filePath);
            var data = ReadFile(filePath);
            LoadData(data);
        }

        private string ReadFile(string filePath)
        {
            var res = "";
            using(var rd = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                res += rd.ReadToEnd();
            }
            return res;
        }

        private void LoadData(string data)
        {
            bool firstRow = true;
            foreach (var row in data.Split(new char[] { '\n' },StringSplitOptions.RemoveEmptyEntries))
            {
                if (firstRow)
                {
                    Columns = row.Split(',').Select(i => i.Trim()).ToList();
                } else
                {
                    var frame = new CsvFrame(row, Columns, IdColumn, TextColumnList);
                    Frames.Add(frame);
                }
                firstRow = false;
                Console.WriteLine("{"+row.Trim()+"}\n");
            }
        }
    }
}
