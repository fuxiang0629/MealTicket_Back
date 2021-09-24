using FXCommon.Common;
using StockMarket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeAPI;

namespace SharesHqService_Level2
{
    class Program
    {

        //12-1-32-99-0-2-19-0-19-0-62-5-5-0-0-0-0-0-0-0-1-0-0-48-48-48-48-48-49

        //static Socket ClientSocket;
        static void Main(string[] args)
        {
            int errorCode = 0;
            int client = 0;
            for (int i = 0; i < 50; i++)
            {
                client = HqService.Hq_Connect("27.151.2.124", 7711, ref errorCode);
            }
            for (int i = 0; i < 50; i++)
            {
                errorCode = 0;
                int count0 = HqService.Hq_GetSecurityCount(i, 0, ref errorCode);
                errorCode = 0;
                int count1 = HqService.Hq_GetSecurityCount(i, 1, ref errorCode);
            }

            //short a = 268;
            //Int32 b = 33579808;
            //short c = 19;
            //short d = 19;
            //Int32 e = 329022;
            //Int32 f = 0;
            //short g = 0;
            //short h = 1;
            //byte i = 0;
            //string j = "000001";

            //byte[] by = new byte[29];
            //BitConverter.GetBytes(a).CopyTo(by,0);
            //BitConverter.GetBytes(b).CopyTo(by, 2);
            //BitConverter.GetBytes(c).CopyTo(by,6);
            //BitConverter.GetBytes(d).CopyTo(by, 8);
            //BitConverter.GetBytes(e).CopyTo(by, 10);
            //BitConverter.GetBytes(f).CopyTo(by, 14);
            //BitConverter.GetBytes(g).CopyTo(by, 18);
            //BitConverter.GetBytes(h).CopyTo(by, 20);
            //BitConverter.GetBytes(i).CopyTo(by, 22);
            //for (int x = 0; x < 6; x++)
            //{
            //    byte[] temp=new byte[1];
            //    temp[0]=BitConverter.GetBytes(j[x]).First();
            //    temp.CopyTo(by, 23 + x);
            //}


            //string IP = "27.151.2.124";
            //int port = 7711;

            //IPAddress ip = IPAddress.Parse(IP);  //将IP地址字符串转换成IPAddress实例
            //ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);//使用指定的地址簇协议、套接字类型和通信协议
            //ClientSocket.ReceiveTimeout = 30000;
            //ClientSocket.Connect(ip, port);  //与远程主机建立连接


            //ClientSocket.Send(strToToHexByte("0c 02 18 93 00 01 03 00 03 00 0d 00 01"));
            //byte[] receive1 = new byte[1024];
            //ClientSocket.Receive(receive1);
            //ClientSocket.Send(strToToHexByte("0c 02 18 94 00 01 03 00 03 00 0d 00 02"));
            //byte[] receive2 = new byte[1024];
            //ClientSocket.Receive(receive2);
            //ClientSocket.Send(strToToHexByte("0c 03 18 99 00 01 20 00 20 00 db 0f d5 d0 c9 cc d6 a4 a8 af 00 00 00 8f c2 25 40 13 00 00 d5 00 c9 cc bd f0 d7 ea 00 00 00 02"));
            //byte[] receive3 = new byte[1024];
            //ClientSocket.Receive(receive3);

            //int RSP_HEADER_LEN = 0x10;
            //while (true)
            //{
            //    Console.WriteLine("开始发送消息");
            //    byte[] message = by;  //通信时实际发送的是字节数组，所以要将发送消息转换字节
            //    ClientSocket.Send(message);
            //    Console.WriteLine("发送消息为:" + BitConverter.ToString(message));

            //    byte[] head_receive = new byte[RSP_HEADER_LEN];
            //    int length = ClientSocket.Receive(head_receive);  // length 接收字节数组长度

            //    if (length == head_receive.Length)
            //    {
            //        //12 - 14;14-16
            //        ushort zipsize = BitConverter.ToUInt16(head_receive, 12);
            //        ushort unzipsize = BitConverter.ToUInt16(head_receive, 14);
            //        List<byte> body_receive = new List<byte>();

            //        while (true)
            //        {
            //            byte[] temp_receive = new byte[zipsize];
            //            int body_length = ClientSocket.Receive(temp_receive);

            //            body_receive.AddRange(temp_receive.Take(body_length).ToArray());
            //            if (body_receive.Count() >= zipsize)
            //            {
            //                break;
            //            }
            //        }
            //        byte[] unzip_receive_body = body_receive.ToArray();
            //        if (zipsize != unzipsize)//解压
            //        {
            //            unzip_receive_body = Utils.Decompress(unzip_receive_body);
            //        }
            //        //int pre= BitConverter.ToUInt16(unzip_receive_body, 0);
            //        int count=BitConverter.ToUInt16(unzip_receive_body, 2);
            //        int market = unzip_receive_body[4];
            //        string sharesCode = BitConverter.ToString(unzip_receive_body,5,6);
            //    }
            //    else
            //    {
            //        Console.WriteLine("head_buf is not 0x10");
            //    }

            //    string exit=Console.ReadLine();
            //    if (exit == "1")
            //    {
            //        break;
            //    }
            //}
            //ClientSocket.Close();  //关闭连接





            //object a = new StringBuilder(1024);
            //StringBuilder b = a as StringBuilder;
            //StringBuilder c = a as StringBuilder;
            //b.Append("1");
            //b.Append("2");


            //StringBuilder sResult = new StringBuilder(1024*1024);
            //StringBuilder sError = new StringBuilder(256);
            //int hqClient = TradeX_M.TdxHq_Connect("27.151.2.124", 7711, sResult, sError);
            //short nCount = 1;
            //byte[] nMarketArr = new byte[] {1,0 };
            //string[] pszZqdmArr = new string[] {"600389","000001" };

            //sResult.Clear();
            //sError.Clear();
            //bool bRet = TradeX_M.TdxHq_GetSecurityList(hqClient, 0, 0, ref nCount, sResult, sError);
            //sResult.Clear();
            //sError.Clear();
            //bRet = TradeX_M.TdxHq_GetSecurityList(hqClient, 1, 0, ref nCount, sResult, sError);
            //bool bRet = TradeX_M.TdxHq_GetSecurityQuotes(hqClient, nMarketArr, pszZqdmArr, ref nCount, sResult, sError);
            //bool bRet = TradeX_M.TdxHq_GetSecurityBars(hqClient, 7, (byte)0, "000001", short.MaxValue, ref nCount, sResult, sError);
            //int connectResult=TradeX_M.TdxL2Hq_Connect("47.103.88.146", 7719, "tdxpc1839777", "l2pwd123", sResult, sError);
            //TradeX_M.TdxL2Hq_Disconnect(connectResult);
        }

        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
    }
}

//{无效License文件! sKey=5a74990ae9ab08b8647376c11cc8fce4, license_key=8781c7c725307b99e92dd1a41f480911}
