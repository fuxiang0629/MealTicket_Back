﻿using FXCommon.Common;
using MealTicket_Web_Handler.Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using stock_db_core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using static StockTrendMonitor.Define.StockMonitorDefine;

namespace MealTicket_Web_Handler
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
        /// 协议对象
        /// </summary>
        IModel _sendModel;

        /// <summary>
        /// 更新协议对象
        /// </summary>
        IModel model;

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
        }

        public void Dispose(bool isDispose = false)
        {
            try
            {
                lock (_lock)
                {
                    if (isDispose)
                    {
                        isConnect = false;
                    }
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                    if (_sendModel != null)
                    {
                        _sendModel.Dispose();
                        _sendModel = null;
                    }
                    if (model != null)
                    {
                        model.Dispose();
                        model = null;
                    }
                }
            }
            catch(Exception ex) 
            {
                Logger.WriteFileLog("这里出错来了", ex);
            }
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
                    if (isConnect)
                    {
                        Dispose();
                        if (_connection == null)
                        {
                            _connection = _factory.CreateConnection();
                            _sendModel = _connection.CreateModel();
                            model = _connection.CreateModel();
                            var consumerUpdate = new EventingBasicConsumer(model);
                            consumerUpdate.Received += Consumer_Received;
                            consumerUpdate.Shutdown += Consumer_Shutdown;
                            model.BasicQos(0, 1, false);
                            model.BasicConsume("TransactionDataTrendAnalyse", false, consumerUpdate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("MQ链接异常", ex);
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

        private bool _SendMessage(byte[] data, string exchange, string routekey)
        {
            try
            {
                lock (_lock)
                {
                    if (_sendModel == null)
                    {
                        return false;
                    }

                    _sendModel.BasicPublish(exchange, routekey, null, data);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 收到更新当天数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body.ToArray());
            try
            {
                if (!DataHelper.CheckHandlerTime())
                {
                    return;
                }
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<List<SharesInfo>>(body);

                List<STOCK_CODE_PARAM> resetList = new List<STOCK_CODE_PARAM>();
                foreach (var sharesInfo in dataJson)
                {
                    resetList.Add(new STOCK_CODE_PARAM
                    {
                        strStockCode = sharesInfo.SharesCode + "," + sharesInfo.Market
                    });
                }
                //刷新数据
                int error = Singleton.Instance.m_stockMonitor.RefreshTradePriceInfoBat(DateTime.Parse(DB_Model.MAX_DATE_TIME), ref resetList);
                if (error != 0)
                {
                    return;
                }
                resetList = resetList.Where(x => x.iErrorCode == 0).ToList();

                List<SharesInfo> successList = new List<SharesInfo>();
                foreach (var item in resetList)
                {
                    var temp = item.strStockCode.Split(',');
                    successList.Add(new SharesInfo
                    {
                        SharesCode = temp[0],
                        Market = int.Parse(temp[1])
                    });
                }
                //分析数据
                DataHelper.ToAnalyse(dataJson);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("执行队列失败", ex);
            }
            finally
            {
                (sender as EventingBasicConsumer).Model.BasicAck(e.DeliveryTag, false);
            }
        }
    }
}
