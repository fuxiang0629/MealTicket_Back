﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Tag_Force_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 类型1.3天  2.5天  3.10天  4.15天
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否上涨1
        /// </summary>
        public bool IsForce1 { get; set; }

        /// <summary>
        /// 是否上涨2
        /// </summary>
        public bool IsForce2 { get; set; }
    }

    public class Plate_Tag_Force_Session_Obj
    {
        public List<Plate_Tag_Force_Session_Info> Force_List { get; set; }
        public Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> Plate_Shares_Force_Session { get; set; }

        public Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> Shares_Plate_Force_Session { get; set; }
    }
}
