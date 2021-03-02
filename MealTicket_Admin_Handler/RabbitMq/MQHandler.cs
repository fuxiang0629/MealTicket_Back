using FXCommon.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text; 
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MealTicket_Admin_Handler
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
        /// 单例模式
        /// </summary>
        public readonly static MQHandler instance = new MQHandler(new ConnectionFactory
        {
            HostName = ConfigurationManager.AppSettings["MQ_HostName"],
            Port = int.Parse(ConfigurationManager.AppSettings["MQ_Port"]),
            UserName = ConfigurationManager.AppSettings["MQ_UserName"],
            Password = ConfigurationManager.AppSettings["MQ_Password"],
            VirtualHost = ConfigurationManager.AppSettings["MQ_VirtualHost"]
        });

        private MQHandler(ConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Dispose()
        {
            if (_sendModel != null)
            {
                _sendModel.Dispose();
            }
            if (_connection != null)
            {
                _connection.Dispose();
            }
        }

        private void Init()
        {
            if (_connection == null)
            {
                lock (_lock)
                {
                    if (_connection == null)
                    {
                        _connection = _factory.CreateConnection();
                        _sendModel = _connection.CreateModel();
                    }
                }
            }
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <returns></returns>
        public List<QueuesMessage> GetMessage(string queueName)
        {
            List<QueuesMessage> messageList = new List<QueuesMessage>();
            string url = string.Format("http://{0}:15672/api/queues/MealTicket/{1}/get", _factory.HostName, queueName);
            string content = JsonConvert.SerializeObject(new
            {
                vhost = _factory.HostName,
                name = queueName,
                truncate = "50000",
                ackmode = "ack_requeue_false",
                encoding = "auto",
                count = 1000,
                requeue=false
            });
            List<QueuesMessage> temp;
            do
            {
                temp = new List<QueuesMessage>();
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_factory.UserName}:{_factory.Password}")));
                    HttpContent httpContent = new StringContent(content, Encoding.UTF8);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    Uri address = new Uri(url);
                    var res = client.PostAsync(address, httpContent).Result.Content.ReadAsStringAsync().Result;//返回值
                    if (res != "[]")
                    {
                        temp = JsonConvert.DeserializeObject<List<QueuesMessage>>(res);
                        messageList.AddRange(JsonConvert.DeserializeObject<List<QueuesMessage>>(res));
                    }
                }
            } while (temp.Count > 0);

            return messageList;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public bool SendMessage(byte[] data, string exchange, string routekey)
        {
            try
            {
                Init();
                lock (this)
                {
                    _sendModel.BasicPublish(exchange, routekey, null, data);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("队列出错了",ex);
                Close();
                return false;
            }
        }

        private void Close()
        {
            if (_sendModel != null)
            {
                _sendModel.Dispose();
            }
            if (_connection != null)
            {
                _connection.Dispose();
            }
            _connection = null;
        }
    }

    public class QueuesMessage 
    {
        public string payload { get; set; }

        public int message_count { get; set; }
    }
}
