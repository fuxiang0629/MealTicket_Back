using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class OptionalTrend
    {
        public long AccountId { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public long TrendId { get; set; }

        public string TrendName { get; set; }

        public string TrendDescription { get; set; }

        public long RelId { get; set; }

        public long OptionalId { get; set; }

        public List<string> ParList { get; set; }
    }

    public class Trend1
    {
        public int ZHFZS { get; set; }

        public List<Trend1Par> ParList { get; set; }
    }

    public class Trend1Par
    {
        public int SZFD { get; set; }

        public int YXFWFD { get; set; }
    }

    public class Trend2
    {
        public int ZDFSSL { get; set; }

        public int ZXFSSL { get; set; }

        public int YXFWFD { get; set; }

        public int YXJJBL { get; set; }

        public int TPFD { get; set; }

        public bool CCJRZGD { get; set; }

        public DateTime CCSDSJ { get; set; }
    }

    public class Trend3
    {
        public int ZXFSSL { get; set; }

        public int TPSFZXBL { get; set; }

        public int YXJJBL { get; set; }

        public bool CCJRZGD { get; set; }

        public int YXJXWC { get; set; }

        public int TPHSJ { get; set; }
    }
}
