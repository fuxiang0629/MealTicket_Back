using FXCommon.Common;
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

namespace TransactionDataTri
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
        /// 触发协议对象
        /// </summary>
        IModel _triModel;

        /// <summary>
        /// 更新协议对象
        /// </summary>
        IModel _updateModel;

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
                    Dispose();
                    if (isConnect)
                    {
                        if (_connection == null)
                        {
                            _connection = _factory.CreateConnection();
                            _sendModel = _connection.CreateModel();

                            _triModel = _connection.CreateModel();
                            var consumerTri = new EventingBasicConsumer(_triModel);
                            consumerTri.Received += Consumer_Received_Tri;
                            consumerTri.Shutdown += Consumer_Shutdown;
                            _triModel.BasicQos(0, 1, false);
                            _triModel.BasicConsume("TransactionDataTri", false, consumerTri);

                            _updateModel = _connection.CreateModel();
                            var consumerUpdate = new EventingBasicConsumer(_updateModel);
                            consumerUpdate.Received += Consumer_Received_Update;
                            consumerUpdate.Shutdown += Consumer_Shutdown;
                            _updateModel.BasicQos(0, 1, false);
                            _updateModel.BasicConsume("TransactionDataFinish", false, consumerUpdate);
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
                    if (_triModel != null)
                    {
                        _triModel.Dispose();
                    }
                    if (_updateModel != null)
                    {
                        _updateModel.Dispose();
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
        /// 收到触发数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Consumer_Received_Tri(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var body = Encoding.UTF8.GetString(e.Body.ToArray());
                Logger.WriteFileLog(body, "Consumer_Received_Tri", null);

                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(body);
                int Market = dataJson.Market;
                string SharesCode = dataJson.SharesCode;

                WaitQuery.Instance.updateWaitQuery(new WaitQueryModel 
                {
                    SharesCode= SharesCode,
                    Market= Market
                },1);
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

        /// <summary>
        /// 收到更新数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Consumer_Received_Update(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var body = Encoding.UTF8.GetString(e.Body.ToArray());
                Logger.WriteFileLog(body, "Consumer_Received_Update", null);

                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(body);
                int Market = dataJson.Market;
                string SharesCode = dataJson.SharesCode;

                WaitQuery.Instance.updateWaitQuery(new WaitQueryModel
                {
                    SharesCode = SharesCode,
                    Market = Market
                }, 3);
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
