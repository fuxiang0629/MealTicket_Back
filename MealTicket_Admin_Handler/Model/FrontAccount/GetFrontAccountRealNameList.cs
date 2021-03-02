using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountRealNameList
    {
    }

    public class GetFrontAccountRealNameListRequest:PageRequest
    {
        /// <summary>
        /// 用户信息（昵称/手机号）
        /// </summary>
        public string AccountInfo { get; set; }

        /// <summary>
        /// 审核状态1审核中 2审核成功 3审核失败
        /// </summary>
        public int ExamineStatus { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class FrontAccountRealNameInfo
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 身份证人头面图片
        /// </summary>
        public string ImgUrlFront { get; set; }

        /// <summary>
        /// 身份证人头面图片网络地址
        /// </summary>
        public string ImgUrlFrontShow
        {
            get
            {
                if (string.IsNullOrEmpty(ImgUrlFront) || ImgUrlFront.ToUpper().StartsWith("HTTP"))
                {
                    return ImgUrlFront;
                }
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], ImgUrlFront);
            }
        }

        /// <summary>
        /// 身份证国徽面图片
        /// </summary>
        public string ImgUrlBack { get; set; }

        /// <summary>
        /// 身份证国徽面图片网络地址
        /// </summary>
        public string ImgUrlBackShow
        {
            get
            {
                if (string.IsNullOrEmpty(ImgUrlBack) || ImgUrlBack.ToUpper().StartsWith("HTTP"))
                {
                    return ImgUrlBack;
                }
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], ImgUrlBack);
            }
        }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 性别 1男 2女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 出生年月
        /// </summary>
        public string BirthDay { get; set; }

        /// <summary>
        /// 身份证号码
        /// </summary>
        public string CardNo { get; set; }

        /// <summary>
        /// 住址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 审核状态1审核中 2审核成功 3审核失败
        /// </summary>
        public int ExamineStatus { get; set; }

        /// <summary>
        /// 审核状态说明
        /// </summary>
        public string ExamineStatusDes { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后审核时间
        /// </summary>
        public DateTime? ExamineFinishTime { get; set; }
    }
}
