using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetConditiontradeTemplateBuyAutoPar
    {
    }

    public class ConditiontradeTemplateBuyAutoParInfo
    {

        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public string ParamsInfo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    public class GetConditiontradeTemplateBuyAutoParPlateRequest:DetailsPageRequest
    {
        /// <summary>
        /// 分组类型1行业 2地区 3概念
        /// </summary>
        public int GroupType { get; set; }

        /// <summary>
        /// 数据类型1翔子 2涨跌幅
        /// </summary>
        public int DataType { get; set; }
    }
}
