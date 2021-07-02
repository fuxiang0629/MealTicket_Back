using FXCommon.Common;
using MealTicket_Web_Handler.Model;
using MealTicket_Web_Handler.Transactiondata;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using stock_db_core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
        IModel _model;

        string _HostName;
        int _Port;
        string _UserName;
        string _Password;
        string _VirtualHost;

        public MQHandler()
        {
            _HostName = ConfigurationManager.AppSettings["MQ_HostName"];
            _Port = int.Parse(ConfigurationManager.AppSettings["MQ_Port"]);
            _UserName = ConfigurationManager.AppSettings["MQ_UserName"];
            _Password = ConfigurationManager.AppSettings["MQ_Password"];
            _VirtualHost = ConfigurationManager.AppSettings["MQ_VirtualHost"];
            _factory = new ConnectionFactory
            {
                HostName = _HostName,
                Port = _Port,
                UserName = _UserName,
                Password = _Password,
                VirtualHost = _VirtualHost
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
                    if (_model != null)
                    {
                        _model.Dispose();
                        _model = null;
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
                            _model = _connection.CreateModel();
                            var consumerUpdate = new EventingBasicConsumer(_model);
                            consumerUpdate.Received += Consumer_Received;
                            consumerUpdate.Shutdown += Consumer_Shutdown;
                            _model.BasicQos(0, 1, false);
                            _model.BasicConsume("TransactionDataTrendAnalyse", false, consumerUpdate);
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
        /// 判断队列是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty(string queryName) 
        {
            var res=_GetMessage(queryName);
            if (res.Count() > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取队列数据
        /// </summary>
        /// <returns></returns>
        private List<dynamic> _GetMessage(string queryName) 
        {
            string url = string.Format("http://{0}:15672/api/queues/MealTicket/{1}/get", _HostName, queryName);
            var content = new
            {
                vhost= _VirtualHost,
                name= queryName,
                truncate =50000,
                ackmode= "ack_requeue_true",
                requeue="true",
                encoding = "auto",
                count=1
            };
            string auth_str = _UserName + ":" + _Password;
            string auth= Convert.ToBase64String(Encoding.Default.GetBytes(auth_str));

            string res=HtmlSender.Post_BasicAuth(JsonConvert.SerializeObject(content), url, auth);
            return JsonConvert.DeserializeObject<List<dynamic>>(res);
        }

        /// <summary>
        /// 清空队列数据
        /// </summary>
        public void clearQueueData(string queryName)
        {
            string url = string.Format("http://{0}:15672/api/queues/MealTicket/{1}/contents", _HostName, queryName);
            var content = new
            {
                vhost = "MealTicket",
                name = "TransactionDataUpdateNew",
                mode = "purge"
            };
            string auth_str = _UserName + ":" + _Password;
            string auth = Convert.ToBase64String(Encoding.Default.GetBytes(auth_str));

            string res = HtmlSender.Post_BasicAuth(JsonConvert.SerializeObject(content), url, auth,"DELETE");
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
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<TransactiondataTaskQueueInfo>(body);
                Singleton.Instance._transactionDataTask.TransactiondataQueue.AddMessage(dataJson);
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
