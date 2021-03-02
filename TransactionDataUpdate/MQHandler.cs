﻿using FXCommon.Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionDataUpdate
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
        List<IModel> modelList = new List<IModel>();

        /// <summary>
        /// 单例模式
        /// </summary>
        public static readonly MQHandler instance = new MQHandler();

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
                lock (_lock)
                {
                    Dispose();
                    if (isConnect)
                    {
                        if (_connection == null)
                        {
                            _connection = _factory.CreateConnection();
                            _sendModel = _connection.CreateModel();

                            for (int i = 0; i < Singleton.Instance.hqClientCount;i++)
                            {
                                var _model = _connection.CreateModel();
                                var consumerUpdate = new EventingBasicConsumer(_model);
                                consumerUpdate.Received += Consumer_Received;
                                consumerUpdate.Shutdown += Consumer_Shutdown;
                                _model.BasicQos(0, 1, false);
                                _model.BasicConsume("TransactionDataUpdate", false, consumerUpdate);
                                modelList.Add(_model);
                            }
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
                    foreach(var _model in modelList)
                    {
                        _model.Dispose();
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
            DateTime timeNow = DateTime.Now;
            int Market = -1;
            string SharesCode = string.Empty;
            int Type=0;
            try
            {
                var body = Encoding.UTF8.GetString(e.Body.ToArray());
                Logger.WriteFileLog(body, "Consumer_Received_Update", null);

                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(body);
                Type = dataJson.Type == null ? 0 : dataJson.Type;
                Market = dataJson.Market;
                SharesCode = dataJson.SharesCode;
                DateTime Date = dataJson.Date == null ? timeNow.Date : DateTime.Parse(dataJson.Date.ToString());

                if (!DataHelper.CheckTradeDate(Date))
                {
                    return;
                }
                if (string.IsNullOrEmpty(SharesCode))
                {
                    return;
                }
                if (Date == timeNow.Date && timeNow < Date.AddHours(9).AddMinutes(25))
                {
                    return;
                }

                DateTime? lastTime;
                var list = DataHelper.TdxHq_GetTransactionData(Market, SharesCode, Date, Type, out lastTime);
                if (list.Count() > 0)
                {
                    lock (Singleton.Instance.TransactionDataLock)
                    {
                        DataHelper.UpdateTransactionData(list, Market, SharesCode, Date, lastTime);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("执行队列失败", ex);
            }
            finally
            {
                (sender as EventingBasicConsumer).Model.BasicAck(e.DeliveryTag, false);
                if (Market != -1 && Type == 0)
                {
                    var sendData = new
                    {
                        Market = Market,
                        SharesCode = SharesCode
                    };
                    MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "Finish");
                }
            }
        }
    }
}
