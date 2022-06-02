using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesCycleManagerList
    {
    }

    public class GetSharesCycleManagerListRequest:PageRequest
    {
        /// <summary>
        /// 上一级周期Id
        /// </summary>
        public long MainCycleId { get; set; }
    }

    public class SharesCycleManagerInfo
    {
        public long Id { get; set; }

        public bool DefaultActive { get; set; }

        public string Name { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndDate { get; set; }

        public long SharesKey { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public List<long> PlateId { get; set; }

        /// <summary>
        /// 线条颜色
        /// </summary>
        public string LineColor { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BgColor { get; set; }

        public List<SharesCycleManagerInfo> Children { get; set; }
    }

    public class SharesCycleManagerDetails
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否默认激活
        /// </summary>
        public bool DefaultActive { get; set; }

        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public List<long> PlateId { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 起始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 周期类型1主周期 2子周期
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 线条颜色
        /// </summary>
        public string LineColor { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        /// 父周期Id
        /// </summary>
        public long FatherId { get; set; }

        /// <summary>
        /// 父周期名称
        /// </summary>
        public string FatherName { get; set; }

        /// <summary>
        /// 子周期数量
        /// </summary>
        public int ChildCount { get; set; }

        /// <summary>
        /// 上一级周期Id
        /// </summary>
        public long MainCycleId { get; set; }
    }

    public class SharesRiseInfo
    {
        /// <summary>
        /// 周期Id
        /// </summary>
        public long CycleId { get; set; }

        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 是否跟股票
        /// </summary>
        public bool IsBase { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndDate { get; set; }

        /// <summary>
        /// 线条颜色
        /// </summary>
        public string LineColor { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        /// 涨跌幅列表
        /// </summary>
        public List<object> RiseRateList { get; set; }

        /// <summary>
        /// 颜色列表
        /// </summary>
        public List<int> ItemColorList { get; set; }
    }

    public class SharesCycleLineDataInfo
    {
        /// <summary>
        /// X轴坐标
        /// </summary>
        public List<string> XData { get; set; }

        /// <summary>
        /// 展示信息
        /// </summary>
        public List<SharesCycleManagerInfo> DataShowList { get; set; }

        /// <summary>
        /// 线条列表
        /// </summary>
        public List<SharesRiseInfo> SharesLineList { get; set; }
    }
}
