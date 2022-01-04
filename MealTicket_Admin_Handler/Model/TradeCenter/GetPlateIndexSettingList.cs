using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetPlateIndexSettingList
    {
    }

    public class PlateIndexSettingInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 类型1龙头指数 2板块联动 3股票联动 4新高指数
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 系数
        /// </summary>
        public int Coefficient { get; set; }

        /// <summary>
        /// 新高计算天数
        /// </summary>
        public int CalDays { get; set; }

        /// <summary>
        /// 新高计算价格类型
        /// </summary>
        public int CalPriceType { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据最后修改时间
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
