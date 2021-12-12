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
                var dataPackage = resultData.PackageList;
                int handlerType = resultData.HandlerType;
                string taskGuid = resultData.TaskGuid;
                int retryCount = resultData.RetryCount;
                int totalRetryCount = resultData.TotalRetryCount;

                if (dataPackage.Count() <= 0)
                {
                    throw new Exception("更新至少需要一只股票数据");
                }
                if (handlerType != 1 && handlerType != 2 && handlerType!=21 && handlerType != 3 && handlerType != 4)
                {
                    throw new Exception("处理类型参数错误");
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Console.WriteLine("==========开始获取K线数据======" + taskGuid + "===");
                Dictionary<int, SecurityBarsDataRes> resultList = new Dictionary<int, SecurityBarsDataRes>();
                var successPackage = new List<SecurityBarsDataParList>();
                var failPackage = new List<SecurityBarsDataParList>();
                int download_Second = 0;
                resultList = DataHelper.TdxHq_GetSecurityBarsData(handlerType,dataPackage,ref failPackage,ref successPackage,ref download_Second);
                stopwatch.Stop();
                Console.WriteLine("=====获取K线数据结束,时间："+ download_Second + "/"+ stopwatch.ElapsedMilliseconds + "============");

                Console.WriteLine("");
                Console.WriteLine("");

                SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
                {
                    PackageList = resultList,
                    TaskGuid = taskGuid,
                    HandlerType = handlerType,
                    RetryCount= retryCount,
                    TotalRetryCount= totalRetryCount,
                    FailPackageList= failPackage,
                    SuccessPackageList= successPackage
                })), "SecurityBars", "update_1min");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
