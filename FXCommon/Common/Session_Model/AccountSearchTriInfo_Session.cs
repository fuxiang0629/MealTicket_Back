using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class AccountSearchTriInfo_Session
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }

        public DateTime LastPushTime { get; set; }

        public int LastPushType { get; set; }

        public int TriCount { get; set; }
    }
}
