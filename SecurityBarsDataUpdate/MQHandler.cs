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
                if (resultData.DataList.Count() <= 0)
                {
                    throw new Exception("更新至少需要一只股票数据");
                }
                if (resultData.DataType != 2 && resultData.DataType != 3 && resultData.DataType != 4 && resultData.DataType != 5 && resultData.DataType != 6 && resultData.DataType != 7 && resultData.DataType != 8 && resultData.DataType != 9 && resultData.DataType != 10 && resultData.DataType != 11)
                {
                    throw new Exception("数据类型参数错误");
                }
                if (resultData.HandlerType != 1 && resultData.HandlerType != 2)
                {
                    throw new Exception("处理类型参数错误");
                }

                object list = new object();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Console.WriteLine("==========开始获取K线数据======" + resultData.TaskGuid + "===");
                list = DataHelper.TdxHq_GetSecurityBarsData(resultData.DataList, (short)resultData.SecurityBarsGetCount, resultData.DataType);
                stopwatch.Stop();
                Console.WriteLine("=====获取K线数据结束:" + stopwatch.ElapsedMilliseconds + "============");

                Console.WriteLine("");
                Console.WriteLine("");

                SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
                {
                    DataList = list,
                    DataType = resultData.DataType,
                    TaskGuid = resultData.TaskGuid,
                    HandlerType = resultData.HandlerType,
                })), "SecurityBars", "update_1min");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
