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
                var resultData = JsonConvert.DeserializeObject<SecurityBarsDataTaskQueueInfo>(data);
                List<SecurityBarsDataInfo> list = new List<SecurityBarsDataInfo>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine("==========开始获取K线数据======" + resultData.TaskGuid + "===");
                list = DataHelper.TdxHq_GetSecurityBarsData(resultData.DataList);
                stopwatch.Stop();
                Console.WriteLine("=====获取K线数据结束:" + stopwatch.ElapsedMilliseconds + "============");
                Console.WriteLine("");
                Console.WriteLine("");

                SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new SecurityBarsDataTaskQueueInfo
                {
                    DataList = list,
                    TaskGuid = resultData.TaskGuid

                })), "SecurityBars", "update_1min");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
