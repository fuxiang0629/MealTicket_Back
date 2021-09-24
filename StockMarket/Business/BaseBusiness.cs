using StockMarket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Business
{
    /// <summary>
    /// 基础业务类
    /// </summary>
    public class BaseBusiness
    {
        Socket ClientSocket;
        object objectLock = new object();

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        internal void Connect(string Ip, int Port, ref int errorCode)
        {
            lock (objectLock)
            {
                errorCode = (int)Error.Success;
                try
                {
                    IPAddress ipAddress = IPAddress.Parse(Ip);  //将IP地址字符串转换成IPAddress实例
                    ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//使用指定的地址簇协议、套接字类型和通信协议
                    ClientSocket.ReceiveTimeout = 15000;
                    ClientSocket.Connect(ipAddress, Port);  //与远程主机建立连接


                    ClientSocket.Send(Utils.HexToByte("0c 02 18 93 00 01 03 00 03 00 0d 00 01"));
                    byte[] receive1 = new byte[1024];
                    ClientSocket.Receive(receive1);
                    ClientSocket.Send(Utils.HexToByte("0c 02 18 94 00 01 03 00 03 00 0d 00 02"));
                    byte[] receive2 = new byte[1024];
                    ClientSocket.Receive(receive2);
                    ClientSocket.Send(Utils.HexToByte("0c 03 18 99 00 01 20 00 20 00 db 0f d5 d0 c9 cc d6 a4 a8 af 00 00 00 8f c2 25 40 13 00 00 d5 00 c9 cc bd f0 d7 ea 00 00 00 02"));
                    byte[] receive3 = new byte[1024];
                    ClientSocket.Receive(receive3);
                }
                catch (Exception ex)
                {
                    errorCode = (int)Error.Connect_Fail;
                }
            }
        }

        /// <summary>
        /// 获取市场内所有证券的数量
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="market"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        internal int Hq_GetSecurityCount(byte market, ref int errorCode)
        {
            lock (objectLock)
            {
                GetSecurityCountApi api = new GetSecurityCountApi();
                byte[] sendData = api.SetRequestParams(market);
                ClientSocket.Send(sendData);

                byte[] response_body = api.ParseResponseHead(ClientSocket, ref errorCode);
                if (errorCode != 0)
                {
                    return -1;
                }

                return api.ParseResponse(response_body);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        internal virtual void Dispose() 
        {
            lock (objectLock)
            {
                if (ClientSocket != null)
                {
                    ClientSocket.Close();  //关闭连接
                }
            }
        }
    }
}
