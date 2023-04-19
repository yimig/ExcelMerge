using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge.Data
{
    public class ReferenceSetting
    {
        public string ReferenceColumn { get; set; }

        public string ReferenceType { get; set; }

        public string DatePattern { get; set; }

        public bool IsFirstFile { get; set; }

        public string ReferenceFileName { get; set; }

        public string FileSource { get; set; }
    }
}
