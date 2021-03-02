using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    public class alipay_trade_refund_response
    {
        public string code { get; set; }

        public string msg { get; set; }

        public string sub_code { get; set; }

        public string sub_msg { get; set; }

        public string refund_amount { get; set; }
    }

    public class ZFB_Refund_Res
    {
        public alipay_trade_refund_response alipay_trade_refund_response { get; set; }


        public alipay_trade_refund_response alipay_trade_fastpay_refund_query_response { get; set; }
    }
}
