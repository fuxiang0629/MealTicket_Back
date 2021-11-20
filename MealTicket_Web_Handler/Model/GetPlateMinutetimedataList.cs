using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetPlateMinutetimedataList
    {
    }

    public class GetPlateMinutetimedataListRequest
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 加权类型1不加权 2总股本加权
        /// </summary>
        public int WeightType { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public DateTime? MaxTime { get; set; }
    }

    public class GetPlateMinutetimedataListRes
    {
        /// <summary>
        /// 是否今日数据
        /// </summary>
        public bool IsToday { get; set; }

        /// <summary>
        /// 数据日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SharesMinutetimedata> List { get; set; }
    }

    public class BatchGetPlateMtLineListRequest
    {
        /// <summary>
        /// 板块列表
        /// </summary>
        public List<BatchGetPlateKLineList_Plate> PlateList { get; set; }
    }

    public class DB_PlateMinutetimeInfo
    {
        public long PlateId { get; set; }

        public long GroupTimeKey { get; set; }

        public DateTime Time { get; set; }

        public long ClosedPrice { get; set; }

        public long YestodayClosedPrice { get; set; }

        public long TradeStock { get; set; }

        public long TradeAmount { get; set; }

        public long LastTradeStock { get; set; }

        public long LastTradeAmount { get; set; }
    }
}
