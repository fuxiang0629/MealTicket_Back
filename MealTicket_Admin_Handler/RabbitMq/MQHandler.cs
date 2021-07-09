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
using FXCommon.MqQueue;

namespace MealTicket_Admin_Handler
{
    public class MQHandler : MQTask
    {
        public MQHandler(string _hostName, int _port, string _userName, string _password, string _virtualHost)
            : base(_hostName, _port, _userName, _password, _virtualHost)
        {
        }

        public override void ReceivedExecute(string data)
        {
           
        }
    }
}
