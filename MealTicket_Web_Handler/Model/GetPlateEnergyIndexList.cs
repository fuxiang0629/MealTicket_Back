using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetPlateEnergyIndexList
    {
    }

    public class GetPlateEnergyIndexListRequest
    {
        /// <summary>
        /// 1.按板块动能 2.按板块涨幅
        /// </summary>
        public int SpeedType { get; set; }
    }

    public class PlateEnergyIndexInfo
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 板块类型
        /// </summary>
        public int PlateType { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateNameOnly { get; set; }

        /// <summary>
        /// 当日涨跌幅
        /// </summary>
        public int PlateRate { get; set; }

        /// <summary>
        /// 综合涨幅
        /// </summary>
        public int OverallRiseRate { get; set; }

        /// <summary>
        /// 板块市场排名
        /// </summary>
        public int PlateRank { get; set; }

        /// <summary>
        /// 动能指数
        /// </summary>
        public int EnergyIndex
        {
            get
            {
                int tempEnergyIndex = 0;
                var EnergyIndexTypeList = Singleton.Instance.EnergyIndexTypeList;
                if (EnergyIndexTypeList.Contains(1))
                {
                    tempEnergyIndex += LeaderIndex.IndexScore;
                }
                if (EnergyIndexTypeList.Contains(2))
                {
                    tempEnergyIndex += PlateLinkageIndex.IndexScore;
                }
                if (EnergyIndexTypeList.Contains(3))
                {
                    tempEnergyIndex += SharesLinkageIndex.IndexScore;
                }
                if (EnergyIndexTypeList.Contains(4))
                {
                    tempEnergyIndex += NewHighIndex.IndexScore;
                }
                return tempEnergyIndex;
            }
        }

        public int EnergyIndexRate
        {
            get
            {
                int Coefficient = 0;
                var EnergyIndexTypeList = Singleton.Instance.EnergyIndexTypeList;
                if (EnergyIndexTypeList.Contains(1))
                {
                    Coefficient += LeaderIndex.Coefficient;
                }
                if (EnergyIndexTypeList.Contains(2))
                {
                    Coefficient += PlateLinkageIndex.Coefficient;
                }
                if (EnergyIndexTypeList.Contains(3))
                {
                    Coefficient += SharesLinkageIndex.Coefficient;
                }
                if (EnergyIndexTypeList.Contains(4))
                {
                    Coefficient += NewHighIndex.Coefficient;
                }
                return Coefficient == 0 ? 0 : (int)Math.Round(EnergyIndex * 1.0 / Coefficient * 10000, 0);
            }
        }

        /// <summary>
        /// 龙头指数
        /// </summary>
        public PlateIndexInfo LeaderIndex { get; set; }

        /// <summary>
        /// 板块联动指数
        /// </summary>
        public PlateIndexInfo PlateLinkageIndex { get; set; }

        /// <summary>
        /// 股票联动指数
        /// </summary>
        public PlateIndexInfo SharesLinkageIndex { get; set; }

        /// <summary>
        /// 新高指数
        /// </summary>
        public PlateIndexInfo NewHighIndex { get; set; }

        /// <summary>
        /// 量比
        /// </summary>
        public PlateVolumeInfo PlateVolume { get; set; }

        /// <summary>
        /// 分钟涨幅
        /// </summary>
        public MinuteRiseInfo MinuteRise { get; set; }
    }

    public class MinuteRiseInfo 
    {
        /// <summary>
        /// 1分钟涨幅
        /// </summary>
        public int Minute1RiseRate { get; set; }

        /// <summary>
        /// 3分钟涨幅
        /// </summary>
        public int Minute3RiseRate { get; set; }

        /// <summary>
        /// 5分钟涨幅
        /// </summary>
        public int Minute5RiseRate { get; set; }

        /// <summary>
        /// 10分钟涨幅
        /// </summary>
        public int Minute10RiseRate { get; set; }

        /// <summary>
        /// 15分钟涨幅
        /// </summary>
        public int Minute15RiseRate { get; set; }

        /// <summary>
        /// 1分钟涨幅排名
        /// </summary>
        public int Minute1RiseRateRank { get; set; }

        /// <summary>
        /// 3分钟涨幅排名
        /// </summary>
        public int Minute3RiseRateRank { get; set; }

        /// <summary>
        /// 5分钟涨幅排名
        /// </summary>
        public int Minute5RiseRateRank { get; set; }

        /// <summary>
        /// 10分钟涨幅排名
        /// </summary>
        public int Minute10RiseRateRank { get; set; }

        /// <summary>
        /// 15分钟涨幅排名
        /// </summary>
        public int Minute15RiseRateRank { get; set; }
    }
}
