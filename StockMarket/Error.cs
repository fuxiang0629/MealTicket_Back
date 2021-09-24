using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket
{
    public enum Error
    {
        Success=0,
        Connect_Fail=50001,
        Connect_NotExist = 50002,
        Response_HeadError = 60001
    }
}
