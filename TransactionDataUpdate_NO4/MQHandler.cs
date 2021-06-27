using FXCommon.Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionDataUpdate_NO4
{
    public sealed class MQHandler
    {
        object _lock = new object();

        bool isConnect = true;

        /// <summary>
        /// 连接工厂
        /// </summary>
        ConnectionFactory _factory;

        /// <summary>
        /// 消息队列连接
        /// </summary>
        public IConnection _connection;

        /// <summary>
        /// 发送协议对象
        /// </summary>
        IModel _sendModel;

        /// <summary>
        /// 更新协议对象
        /// </summary>
        IModel model;

        /// <summary>
        /// 单例模式
        /// </summary>
        public static readonly MQHandler Instance = new MQHandler();

        private MQHandler()
        {
            _factory = new ConnectionFactory
            {
                HostName = ConfigurationManager.AppSettings["MQ_HostName"],
                Port = int.Parse(ConfigurationManager.AppSettings["MQ_Port"]),
                UserName = ConfigurationManager.AppSettings["MQ_UserName"],
                Password = ConfigurationManager.AppSettings["MQ_Password"],
                VirtualHost = ConfigurationManager.AppSettings["MQ_VirtualHost"]
            };
        }

        /// <summary>
        /// 重连连接和发送通道
        /// </summary>
        public void Reconnect()
        {
            try
            {
                Dispose();
                lock (_lock)
                {
                    if (isConnect)
                    {
                        if (_connection == null)
                        {
                            _connection = _factory.CreateConnection();
                            _sendModel = _connection.CreateModel();

                            model = _connection.CreateModel();
                            var consumerUpdate = new EventingBasicConsumer(model);
                            consumerUpdate.Received += Consumer_Received;
                            consumerUpdate.Shutdown += Consumer_Shutdown;
                            model.BasicQos(0, 1, false);
                            model.BasicConsume("TransactionDataUpdateNew", false, consumerUpdate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(5000);
                Reconnect();
                Logger.WriteFileLog("MQ链接异常", ex);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public bool SendMessage(byte[] data, string exchange, string routekey)
        {
            try
            {
                lock (this)
                {
                    _sendModel.BasicPublish(exchange, routekey, null, data);
                }
                return true;
            }
            catch (Exception ex)
            {
                Reconnect();
                return false;
            }
        }

        /// <summary>
        /// 连接断开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Consumer_Shutdown(object sender, ShutdownEventArgs e)
        {
            Reconnect();
            Logger.WriteFileLog("队列链接断开重连", null);
        }

        /// <summary>
        /// 销毁连接和发送通道
        /// </summary>
        public void Dispose(bool isDispose = false)
        {
            try
            {
                lock (_lock)
                {
                    if (model != null)
                    {
                        model.Dispose();
                    }
                    if (_sendModel != null)
                    {
                        _sendModel.Dispose();
                    }
                    if (_connection != null)
                    {
                        _connection.Dispose();
                    }
                    _connection = null;
                    if (isDispose)
                    {
                        isConnect = false;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 收到更新当天数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var body = Encoding.UTF8.GetString(e.Body.ToArray());

                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<List<dynamic>>(body);

                List<SharesInfo> sharesList = new List<SharesInfo>();
                foreach (var item in dataJson)
                {
                    string sharesCode = item.SharesCode;
                    int market = item.Market;
                    sharesList.Add(new SharesInfo
                    {
                        SharesCode = sharesCode,
                        Market = market,
                        SharesInfoNum = int.Parse(sharesCode) * 10 + market
                    });
                }
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine("==========开始获取分笔数据=================");
                var list = DataHelper.TdxHq_GetTransactionData(sharesList);
                stopwatch.Stop();
                Console.WriteLine("=====获取分笔数据结束:" + stopwatch.ElapsedMilliseconds + "============");
                Console.WriteLine("==========开始更新分笔数据=================");
                stopwatch.Restart();
                bool isUpdateSuccess = false;
                if (list.Count() > 0)
                {
                    isUpdateSuccess = DataHelper.UpdateTransactionData(list);
                }
                stopwatch.Stop();
                Console.WriteLine("=====分笔数据更新结束:" + stopwatch.ElapsedMilliseconds + "============");
                Console.WriteLine("");
                Console.WriteLine("");

                if (isUpdateSuccess)
                {
                    SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sharesList)), "TransactionData", "TrendAnalyse");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分笔数据更新失败", ex);
            }
            finally
            {
                (sender as EventingBasicConsumer).Model.BasicAck(e.DeliveryTag, false);
            }
        }
    }
}
