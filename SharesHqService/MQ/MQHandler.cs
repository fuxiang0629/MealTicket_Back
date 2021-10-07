using FXCommon.Common;
using FXCommon.MqQueue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharesHqService
{
    public class MQHandler:MQTask
    {
        public MQHandler(string _hostName, int _port, string _userName, string _password, string _virtualHost) 
            : base(_hostName, _port, _userName, _password, _virtualHost)
        {
        }

        public override void ReceivedExecute(string data)
        {
            throw new NotImplementedException();
        }
    }
}
