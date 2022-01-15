using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesEnergyIndexList
    {
    }

    public class SharesEnergyIndex : IComparable<SharesEnergyIndex>
    {
        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

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
        /// 触发时间
        /// </summary>
        public DateTime TriTime{ get; set; }

        /// <summary>
        /// 触发次数
        /// </summary>
        public int TriCount { get; set; }

        /// <summary>
        /// 文字颜色
        /// </summary>
        public string TextColor
        {
            get
            {
                DateTime timeNow = DateTime.Now;
                int intervalSecond = (int)(timeNow - TriTime).TotalSeconds;
                if (TriCount > 1)
                {
                    if (intervalSecond < 60)
                    {
                        return "rgb(6, 88, 241)";
                    }
                    else if (intervalSecond < 120)
                    {
                        return "rgb(95, 148, 247)";
                    }
                    else if (intervalSecond < 180)
                    {
                        return "rgb(161, 190, 244)";
                    }
                    else
                    {
                        return "-1";
                    }
                }
                else
                {
                    if (intervalSecond < 60)
                    {
                        return "rgb(255, 0, 0)";
                    }
                    else if (intervalSecond < 120)
                    {
                        return "rgb(255, 100, 100)";
                    }
                    else if (intervalSecond < 180)
                    {
                        return "rgb(255, 200, 200)";
                    }
                    else
                    {
                        return "-1";
                    }
                }
            }
        }


        /// <summary>
        /// 是否炸板
        /// </summary>
        public bool IsLimitUpBomb { get; set; }

        /// <summary>
        /// 股票当前涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 股票综合涨跌幅
        /// </summary>
        public int OverallRiseRate { get; set; }

        /// <summary>
        /// 股票综合排名
        /// </summary>
        public int OverallRank { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 板块内股票排名信息
        /// </summary>
        public List<PlateSharesRank> PlateSharesRank { get; set; }


        public int CompareTo(SharesEnergyIndex other)
        {
            return (other.TriTime.CompareTo(this.TriTime) > 0 ? 1 : -1);
        }
    }
}
