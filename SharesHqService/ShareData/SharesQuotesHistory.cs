using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class SharesQuotesHistory
    {
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public string Date { get; set; }
        public int Type { get; set; }
        public int BusinessType { get; set; }
        public DateTime CreateTime { get; set; }

    }
}
