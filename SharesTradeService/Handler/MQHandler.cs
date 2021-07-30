using FXCommon.Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;

namespace SharesTradeService.Handler
{
    public sealed class MQHandler
    {
        object _lock = new object();

        /// <summary>
        /// 连接工厂
        /// </summary>
        ConnectionFactory _factory;

        /// <summary>
        /// 消息队列连接
        /// </summary>
        public IConnection _connection;

        /// <summary>
        /// 协议对象
        /// </summary>
        IModel _sendModel;

        /// <summary>
        /// 购买渠道列表
        /// </summary>
        List<IModel> _readBuyModelList;

        /// <summary>
        /// 购买渠道数量
        /// </summary>
        int buyModelCount = 1;

        /// <summary>
        /// 卖出渠道列表
        /// </summary>
        List<IModel> _readSellModelList;

        /// <summary>
        /// 卖出渠道数量
        /// </summary>
        int sellModelCount = 1;

        /// <summary>
        /// 撤单渠道列表
        /// </summary>
        List<IModel> _readCancelModelList;

        /// <summary>
        /// 卖出渠道数量
        /// </summary>
        int cancelModelCount = 1;

        /// <summary>
        /// 查询渠道列表
        /// </summary>
        List<IModel> _readQueryModelList;

        /// <summary>
        /// 查询渠道数量
        /// </summary>
        int queryModelCount = 1;

        public MQHandler()
        {
            _factory = new ConnectionFactory
            {
                HostName = ConfigurationManager.AppSettings["MQ_HostName"],
                Port = int.Parse(ConfigurationManager.AppSettings["MQ_Port"]),
                UserName = ConfigurationManager.AppSettings["MQ_UserName"],
                Password = ConfigurationManager.AppSettings["MQ_Password"],
                VirtualHost = ConfigurationManager.AppSettings["MQ_VirtualHost"]
            };
            Reconnect();
        }

        /// <summary>
        /// 重连连接和发送通道
        /// </summary>
        public void Reconnect()
        {
            try
            {
                lock (_lock)
                {
                    if (_connection == null)
                    {
                        _connection = _factory.CreateConnection();
                        _sendModel = _connection.CreateModel();
                        _readBuyModelList = new List<IModel>();
                        for (int i = 0; i < buyModelCount; i++)
                        {
                            var tempModel = _connection.CreateModel();
                            _readBuyModelList.Add(tempModel);
                            var consumer = new EventingBasicConsumer(tempModel);
                            consumer.Received += BuyConsumer_Received;
                            consumer.Shutdown += Consumer_Shutdown;
                            tempModel.BasicQos(0, 1, false);
                            tempModel.BasicConsume("SharesBuy_" + Singleton.instance.ServerId, true, consumer);
                        }
                        _readSellModelList = new List<IModel>();
                        for (int i = 0; i < sellModelCount; i++)
                        {
                            var tempModel = _connection.CreateModel();
                            _readSellModelList.Add(tempModel);
                            var consumer = new EventingBasicConsumer(tempModel);
                            consumer.Received += SellConsumer_Received;
                            consumer.Shutdown += Consumer_Shutdown;
                            tempModel.BasicQos(0, 1, false);
                            tempModel.BasicConsume("SharesSell_" + Singleton.instance.ServerId, true, consumer);
                        }
                        _readCancelModelList = new List<IModel>();
                        for (int i = 0; i < cancelModelCount; i++)
                        {
                            var tempModel = _connection.CreateModel();
                            _readCancelModelList.Add(tempModel);
                            var consumer = new EventingBasicConsumer(tempModel);
                            consumer.Received += CancelConsumer_Received;
                            consumer.Shutdown += Consumer_Shutdown;
                            tempModel.BasicQos(0, 1, false);
                            tempModel.BasicConsume("SharesCancel_" + Singleton.instance.ServerId, true, consumer);
                        }
                        _readQueryModelList = new List<IModel>();
                        for (int i = 0; i < queryModelCount; i++)
                        {
                            var tempModel = _connection.CreateModel();
                            _readQueryModelList.Add(tempModel);
                            var consumer = new EventingBasicConsumer(tempModel);
                            consumer.Received += QueryConsumer_Received;
                            consumer.Shutdown += Consumer_Shutdown;
                            tempModel.BasicQos(0, 1, false);
                            tempModel.BasicConsume("SharesQuery_" + Singleton.instance.ServerId, true, consumer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                Thread.Sleep(5000);
                Reconnect();
                Logger.WriteFileLog("MQ链接异常",ex);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public bool SendMessage(byte[] data, string exchange, string routekey)
        {
            if (_SendMessage(data, exchange, routekey))
            {
                return true;
            }

            Reconnect();
            return false;
        }

        private bool _SendMessage(byte[] originalData, string exchange, string routekey)
        {
            try
            {
                lock (_lock)
                {
                    if (_sendModel == null)
                    {
                        return false;
                    }

                    bool isCompressed = (originalData.Length > 16 * 1024);
                    byte[] newData = (isCompressed ? Utils.Compress(originalData) : originalData);
                    byte[] sendData = Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new { isCompressed = isCompressed, data = newData }));
                    _sendModel.BasicPublish(exchange, routekey, null, sendData);
                }
                return true;
            }
            catch (Exception ex)
            {
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
            lock (_lock)
            {
                if (_connection!=null && !_connection.IsOpen)
                {
                    Dispose();
                }
                _connection = null;
                Reconnect();
            }
            Logger.WriteFileLog("队列链接断开重连",null);
        }

        /// <summary>
        /// 收到购买数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BuyConsumer_Received(object sender, BasicDeliverEventArgs e)
        {
            Logger.WriteFileLog("数据消费队列,时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RabbitMq/consumption/SharesBuy_" + Singleton.instance.ServerId, null);
            byte[] recvedData = e.Body.ToArray();
            var resultParse = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(recvedData));
            byte[] newData = ((bool)resultParse.isCompressed ? Utils.Decompress((byte[])resultParse.data) : resultParse.data);
            try
            {
                DateTime timeDate = DateTime.Now.Date;
                List<dynamic> buyList = new List<dynamic>();
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(newData));
                if (dataJson.type == 1)
                {
                    foreach (var item in dataJson.data)
                    {
                        long entrustId = item.BuyId;
                        string buyTime = item.BuyTime;
                        int buyCount = item.BuyCount;
                        //买入条件判断
                        if (timeDate != DateTime.Parse(buyTime))//委托过期
                        {
                            return;
                        }
                        buyList.Add(new
                        {
                            entrustId = entrustId,
                            buyCount = buyCount
                        });
                    }
                }
                else
                {
                    long entrustId = dataJson.BuyId;
                    string buyTime = dataJson.BuyTime;
                    int buyCount = dataJson.BuyCount;
                    //买入条件判断
                    if (timeDate != DateTime.Parse(buyTime))//委托过期
                    {
                        return;
                    }
                    buyList.Add(new 
                    {
                        entrustId= entrustId,
                        buyCount= buyCount
                    });
                }
                //买入动作
                SharesTradeBusiness.BuyTrade(buyList);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("执行购买队列失败",ex);
                return;
            }
        }

        /// <summary>
        /// 收到卖出数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SellConsumer_Received(object sender, BasicDeliverEventArgs e)
        {
            Logger.WriteFileLog("数据消费队列,时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RabbitMq/consumption/SharesSell_" + Singleton.instance.ServerId, null);
            byte[] recvedData = e.Body.ToArray();
            var resultParse = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(recvedData));
            byte[] newData = ((bool)resultParse.isCompressed ? Utils.Decompress((byte[])resultParse.data) : resultParse.data);
            try
            {
                DateTime timeDate = DateTime.Now.Date;
                List<long> sellManagerIdList = new List<long>();
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(newData));

                if (dataJson.type == 1)
                {
                    foreach (var item in dataJson.data)
                    {
                        long sellManagerId = item.SellManagerId;
                        string sellTime = item.SellTime;

                        //卖出条件判断
                        if (timeDate != DateTime.Parse(sellTime))//委托过期
                        {
                            return;
                        }
                        sellManagerIdList.Add(sellManagerId);
                    }
                }
                else
                {

                    long sellManagerId = dataJson.SellManagerId;
                    string sellTime = dataJson.SellTime;

                    //卖出条件判断
                    if (timeDate != DateTime.Parse(sellTime))//委托过期
                    {
                        return;
                    }
                    sellManagerIdList.Add(sellManagerId);
                }
                //卖出动作
                SharesTradeBusiness.SellTrade(sellManagerIdList);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("执行卖出队列失败", ex);
                return;
            }
        }

        /// <summary>
        /// 收到撤单数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelConsumer_Received(object sender, BasicDeliverEventArgs e)
        {
            Logger.WriteFileLog("数据消费队列,时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RabbitMq/consumption/CancelSell_" + Singleton.instance.ServerId, null);
            byte[] recvedData = e.Body.ToArray();
            var resultParse = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(recvedData));
            byte[] newData = ((bool)resultParse.isCompressed ? Utils.Decompress((byte[])resultParse.data) : resultParse.data);
            try
            {
                DateTime timeDate = DateTime.Now.Date;
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(newData));
                long tradeManagerId = dataJson.TradeManagerId;
                long entrustId = dataJson.EntrustId;
                string cancelTime = dataJson.CancelTime;
                //买入条件判断
                if (timeDate != DateTime.Parse(cancelTime))//委托过期
                {
                    //确认队列
                    return;
                }
                //买入动作
                SharesTradeBusiness.CancelTrade(tradeManagerId, entrustId);
                (sender as EventingBasicConsumer).Model.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("执行撤单队列失败", ex);
                return;
            }
        }

        /// <summary>
        /// 收到同步数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryConsumer_Received(object sender, BasicDeliverEventArgs e)
        {
            byte[] recvedData = e.Body.ToArray();
            var resultParse = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(recvedData));
            byte[] newData = ((bool)resultParse.isCompressed ? Utils.Decompress((byte[])resultParse.data) : resultParse.data);
            try
            {
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(newData));
                string tradeAccountCode = dataJson.TradeAccountCode;
                //查询动作
                SharesTradeBusiness.QueryTrade(tradeAccountCode);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("执行同步数据队列失败", ex);
                return;
            }
        }

        /// <summary>
        /// 销毁连接和发送通道
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_sendModel != null)
                {
                    _sendModel.Dispose();
                }
                if (_readBuyModelList != null)
                {
                    foreach (var item in _readBuyModelList)
                    {
                        if (item != null)
                        {
                            item.Dispose();
                        }
                    }
                }
                if (_readSellModelList != null)
                {
                    foreach (var item in _readSellModelList)
                    {
                        if (item != null)
                        {
                            item.Dispose();
                        }
                    }
                }
                if (_readCancelModelList != null)
                {
                    foreach (var item in _readCancelModelList)
                    {
                        if (item != null)
                        {
                            item.Dispose();
                        }
                    }
                }

                if (_readQueryModelList != null)
                {
                    foreach (var item in _readQueryModelList)
                    {
                        if (item != null)
                        {
                            item.Dispose();
                        }
                    }
                }
                if (_connection != null)
                {
                    _connection.Dispose();
                }
                _connection = null;
            }
            catch { }
        }
    }
}
