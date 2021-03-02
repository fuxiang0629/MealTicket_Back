﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class SendVerifyCode
    {
    }

    public class SendVerifyCodeRequest
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 业务类型1.注册 2.找回密码 3.修改手机号 4.绑定银行卡
        /// </summary>
        public int BusinessType { get; set; }

        /// <summary>
        /// 图片验证码
        /// </summary>
        public string PicVerifycode { get; set; }
    }
}
