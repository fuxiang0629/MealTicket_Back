using StockMarket.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket
{
    public class HqService
    {
        static int MaxClient = 0;//当前最大连接Id

        static ThreadMsgTemplate<int> FreeClientQueue = new ThreadMsgTemplate<int>();//空闲连接Id队列
        static HqService()
        {
            FreeClientQueue.Init();
        }

        static object ConnDicLock = new object();
        static Dictionary<int, BaseBusiness> ConnDic = new Dictionary<int, BaseBusiness>();

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="Ip">Ip地址</param>
        /// <param name="Port">端口</param>
        /// <param name="errorCode">错误代码</param>
        /// <returns></returns>
        public static int Hq_Connect(string Ip,int Port,ref int errorCode)
        {
            int connId = 0;
            lock (ConnDicLock)
            {
                if (!FreeClientQueue.GetMessage(ref connId,true))
                {
                    connId = MaxClient;
                    MaxClient++;
                }
            }
            ConnDic[connId] = new BaseBusiness();
            ConnDic[connId].Connect(Ip, Port, ref errorCode);
            if (errorCode != 0)
            {
                Hq_Disconnect(connId);
                return -1;
            }
            return connId;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="connId">连接Id</param>
        /// <returns></returns>
        public static bool Hq_Disconnect(int connId)
        {
            lock (ConnDicLock)
            {
                if (!ConnDic.ContainsKey(connId))
                {
                    return false;
                }
                ConnDic[connId].Dispose();
                ConnDic.Remove(connId);
                FreeClientQueue.AddMessage(connId,false);
                return true;
            }
        }

        /// <summary>
        /// 获取市场内所有证券的数量
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="market"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static int Hq_GetSecurityCount(int connId, byte market, ref int errorCode)
        {
            lock (ConnDicLock)
            {
                if (!ConnDic.ContainsKey(connId))
                {
                    errorCode = (int)Error.Connect_NotExist;
                    return -1;
                }
            }
            return ConnDic[connId].Hq_GetSecurityCount(market, ref errorCode);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public static void Dispose() 
        {
            FreeClientQueue.Release();
        }
    }
}
