using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge.Data
{
    public class CsvFrame
    {
        public string Id { get; set; }
        public Dictionary<string, decimal> DecimalDict { get; set; }
        public Dictionary<string, string> TextDict { get; set; }

        private List<string> TextColumnList { get; set; }

        public CsvFrame(string row,List<string> column,string idColumn, List<string> textColumnList) 
        {
            DecimalDict = new Dictionary<string, decimal>();
            TextDict = new Dictionary<string, string>();
            TextColumnList = new List<string>();
            textColumnList.ForEach(i => TextColumnList.Add(i));
            TextColumnList.Add(idColumn);
            LoadDict(row,column);
            LoadId(idColumn);
        }

        public void LoadDict(string row,List<string> column)
        {
            var rowList = row.Split(',').Select(i => i.Trim()).ToList();
            for(int i = 0; i < rowList.Count(); i++)
            {
                decimal temp;
                if (!TextColumnList.Contains(column[i]) && decimal.TryParse(rowList[i], out temp))
                {
                    DecimalDict.Add(column[i], temp);
                }
                else
                {
                    TextDict.Add(column[i], rowList[i]);
                }
            }
        }

        private void LoadId(string idColumn)
        {
            Id = TextDict[idColumn];
        }
    }
}
