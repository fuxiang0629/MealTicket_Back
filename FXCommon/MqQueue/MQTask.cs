using FXCommon.Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FXCommon.MqQueue
{
    public abstract class MQTask
    {
        /// <summary>
        /// 连接工厂
        /// </summary>
        ConnectionFactory _factory;

        /// <summary>
        /// 消息队列连接
        /// </summary>
        IConnection _connection;

        /// <summary>
        /// 发送协议对象
        /// </summary>
        IModel _sendModel;

        /// <summary>
        /// 接收协议对象
        /// </summary>
        IModel _listenModel;

        bool isConnect = true;

        /// <summary>
        /// 发送通道是否有效
        /// </summary>
        bool isSendModelValid = false;

        /// <summary>
        /// 发送通道对象锁
        /// </summary>
        object sendModelLock = new object();

        /// <summary>
        /// 监听数据通道对象锁
        /// </summary>
        object listenModellLock = new object();

        /// <summary>
        /// 监听队列名称
        /// </summary>
        public string ListenQueueName;

        /// <summary>
        /// 数据压缩大小限制,默认4kb（单位：byte）
        /// </summary>
        public int CompressedSize;

        string HostName;
        int Port;
        string UserName;
        string Password;
        string VirtualHost;

        public MQTask(string _hostName, int _port, string _userName, string _password, string _virtualHost)
        {
            CompressedSize = 4*1024;
            HostName = _hostName;
            Port = _port;
            UserName = _userName;
            Password = _password;
            VirtualHost = _virtualHost;
            _factory = new ConnectionFactory
            {
                HostName = HostName,
                Port = Port,
                UserName = UserName,
                Password = Password,
                VirtualHost = VirtualHost
            };
            _connection = _factory.CreateConnection();
            SendModelConnect();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="originalData">发送数据</param>
        /// <param name="exchange">exchange</param>
        /// <param name="routekey">routekey</param>
        /// <returns></returns>
        public bool SendMessage(byte[] originalData, string exchange, string routekey)
        {
            bool sendSuccess = false;
            bool isCompressed = (originalData.Length > CompressedSize);
            byte[] newData = (isCompressed ? Utils.Compress(originalData) : originalData);
            byte[] sendData = Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new { isCompressed = isCompressed, data = newData }));

            lock (sendModelLock)
            {
                try
                {
                    if (isSendModelValid)
                    {
                        _sendModel.BasicPublish(exchange, routekey, null, sendData);
                        sendSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("队列消息发送失败", ex);
                }
                if (!sendSuccess)
                {
                    isSendModelValid = false;
                    SendModelConnect();
                }
                Logger.WriteFileLog("数据加入队列" + (sendSuccess ? "成功" : "失败") + ",时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RabbitMq/production/" + exchange + "/" + routekey, null);
                return sendSuccess;
            }
        }

        /// <summary>
        /// 发送渠道连接
        /// </summary>
        private void SendModelConnect()
        {
            try
            {
                lock (sendModelLock)
                {
                    if (isSendModelValid)
                    {
                        return;
                    }
                    _sendModel = _connection.CreateModel();
                    isSendModelValid = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("发送通道连接失败",ex);
            }
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        /// <returns></returns>
        public bool StartListen() 
        {
            return ListenModelConnect(false);
        }

        /// <summary>
        /// 监听数据通道连接
        /// </summary>
        private bool ListenModelConnect(bool isRetry) 
        {
            lock (listenModellLock)
            {
                if (!isConnect)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(ListenQueueName))
                {
                    Logger.WriteFileLog("启动队列监听出错，监听队列名称为空", null);
                    return false;
                }
                try
                {
                    _listenModel = _connection.CreateModel();
                    var consumerUpdate = new EventingBasicConsumer(_listenModel);
                    consumerUpdate.Received += Consumer_Received;
                    consumerUpdate.Shutdown += Consumer_Shutdown;
                    _listenModel.BasicQos(0, 1, false);
                    _listenModel.BasicConsume(ListenQueueName, false, consumerUpdate);
                    Logger.WriteFileLog("监听队列已链接", null);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("启动队列监听出错", ex);
                    if (isRetry)
                    {
                        Thread.Sleep(3000);
                        ListenModelConnect(true);
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 连接断开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Consumer_Shutdown(object sender, ShutdownEventArgs e)
        {
            Logger.WriteFileLog("监听队列链接断开", null);
            ListenModelConnect(true);
        }

        /// <summary>
        /// 收到更新当天数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            Logger.WriteFileLog("数据消费队列,时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RabbitMq/consumption/" + ListenQueueName, null);
            try
            {
                byte[] recvedData = e.Body.ToArray();
                var resultParse = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(recvedData));
                byte[] newData = ((bool)resultParse.isCompressed ? Utils.Decompress((byte[])resultParse.data) : resultParse.data);
                //执行业务
                ReceivedExecute(Encoding.UTF8.GetString(newData));
               
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
        /// 清空队列数据
        /// </summary>
        public void ClearQueueData(string queryName)
        {
            string url = string.Format("http://{0}:15672/api/queues/MealTicket/{1}/contents", HostName, queryName);
            var content = new
            {
                vhost = VirtualHost,
                name = queryName,
                mode = "purge"
            };
            string auth_str = UserName + ":" + Password;
            string auth = Convert.ToBase64String(Encoding.Default.GetBytes(auth_str));

            string res = HtmlSender.Post_BasicAuth(JsonConvert.SerializeObject(content), url, auth, "DELETE");
        }

        /// <summary>
        /// 获取队列数据
        /// </summary>
        /// <returns></returns>
        public  List<dynamic> GetMessage(string queryName)
        {
            string url = string.Format("http://{0}:15672/api/queues/MealTicket/{1}/get", HostName, queryName);
            var content = new
            {
                vhost = VirtualHost,
                name = queryName,
                truncate = 50000,
                ackmode = "ack_requeue_true",
                requeue = "true",
                encoding = "auto",
                count = 1000
            };
            string auth_str = UserName + ":" + Password;
            string auth = Convert.ToBase64String(Encoding.Default.GetBytes(auth_str));

            string res = HtmlSender.Post_BasicAuth(JsonConvert.SerializeObject(content), url, auth);
            return JsonConvert.DeserializeObject<List<dynamic>>(res);
        }

        /// <summary>
        /// 判断队列是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty(string queryName)
        {
            var res = GetMessage(queryName);
            if (res.Count() > 0)
            {
                return false;
            }
            return true;
        }

        public abstract void ReceivedExecute(string data);

        public virtual void Dispose()
        {
            isConnect = false;
            try
            {
                lock (sendModelLock)
                {
                    if (_sendModel != null)
                    {
                        _sendModel.Dispose();
                        _sendModel = null;
                    }
                }
                lock (listenModellLock)
                {
                    if (_listenModel != null)
                    {
                        _listenModel.Dispose();
                        _listenModel = null;
                    }
                }
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("这里出错来了", ex);
            }
        }
    }
}
