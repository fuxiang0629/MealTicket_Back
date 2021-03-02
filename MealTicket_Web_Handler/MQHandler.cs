using FXCommon.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace MealTicket_Web_Handler
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
}
