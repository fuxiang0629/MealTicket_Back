using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountModifyBaseInfo
    {
    }

    public class AccountModifyBaseInfoRequest 
    {
        /// <summary>
        /// 修改值
        /// 头像：相对地址
        /// 性别（0未知 1男 2女）
        /// 出生年月（2020-01-01）
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 类型1头像 2昵称 3性别 4出生年月
        /// </summary>
        public int Type { get; set; }
    }
}
