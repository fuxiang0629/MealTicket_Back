using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountOptionalGroupRelList
    {
    }

    public class GetAccountOptionalGroupRelListRequest:DetailsPageRequest
    { 
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }
    }

    public class AccountOptionalGroupRelInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 是否持续
        /// </summary>
        public bool GroupIsContinue { get; set; }

        /// <summary>
        /// 有效期起始时间
        /// </summary>
        public DateTime? ValidStartTimeDt { get; set; }

        /// <summary>
        /// 有效期截止时间
        /// </summary>
        public DateTime? ValidEndTimeDt { get; set; }

        /// <summary>
        /// 有效期起始时间
        /// </summary>
        public string ValidStartTime
        {
            get 
            {
                if (ValidStartTimeDt == null)
                {
                    return null;
                }
                return ValidStartTimeDt.Value.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 有效期截止时间
        /// </summary>
        public string ValidEndTime
        {
            get
            {
                if (ValidEndTimeDt == null)
                {
                    return null;
                }
                return ValidEndTimeDt.Value.ToString("yyyy-MM-dd");
            }
        }
    }
}
