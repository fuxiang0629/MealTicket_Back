using FXCommon.Common;
using FXCommon.MqQueue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    public class MQHandler : MQTask
    {
        public MQHandler(string _hostName, int _port, string _userName, string _password, string _virtualHost)
            : base(_hostName, _port, _userName, _password, _virtualHost)
        {
        }

        public override void ReceivedExecute(string data)
        {
            try
            {
                var resultData = JsonConvert.DeserializeObject<SecurityBarsDataTaskInfo>(data);
                if (resultData.SecurityBarsGetCount <= 0)
                {
                    resultData.SecurityBarsGetCount = Singleton.Instance.SecurityBarsGetCount;
                }
                if (string.IsNullOrEmpty(resultData.QueueRouteKey))
                {
                    resultData.QueueRouteKey = "update_1min";
                }
                List<SecurityBarsDataInfo> list = new List<SecurityBarsDataInfo>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine("==========开始获取K线数据======" + resultData.TaskGuid + "===");
                list = DataHelper.TdxHq_GetSecurityBarsData(resultData.DataList, (short)resultData.SecurityBarsGetCount, resultData.Date);
                stopwatch.Stop();
                Console.WriteLine("=====获取K线数据结束:" + stopwatch.ElapsedMilliseconds + "============");
                Console.WriteLine("");
                Console.WriteLine("");

                SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
                {
                    DataList = list,
                    Date = resultData.Date,
                    TaskGuid = resultData.TaskGuid
                })), "SecurityBars", resultData.QueueRouteKey);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
