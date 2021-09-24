using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.API
{
    /// <summary>
    /// 基础API
    /// </summary>
    public class BaseApi
    {
        int RSP_HEADER_LEN = 0x10;

        /// <summary>
        /// 解析头部
        /// </summary>
        /// <returns></returns>
        internal byte[] ParseResponseHead(Socket ClientSocket,ref int errorCode) 
        {
            errorCode = (int)Error.Success;

            byte[] head_receive = new byte[RSP_HEADER_LEN];
            int length = ClientSocket.Receive(head_receive);  // length 接收字节数组长度

            if (length == head_receive.Length)
            {
                ushort zipsize = BitConverter.ToUInt16(head_receive, 12);
                ushort unzipsize = BitConverter.ToUInt16(head_receive, 14);
                List<byte> body_receive = new List<byte>();

                while (true)
                {
                    byte[] temp_receive = new byte[zipsize];
                    int body_length = ClientSocket.Receive(temp_receive);

                    body_receive.AddRange(temp_receive.Take(body_length).ToArray());
                    if (body_receive.Count() >= zipsize)
                    {
                        break;
                    }
                }
                byte[] unzip_receive_body = body_receive.ToArray();
                if (zipsize != unzipsize)//解压
                {
                    unzip_receive_body = Utils.Decompress(unzip_receive_body);
                }
                return unzip_receive_body;
            }
            else
            {
                errorCode = (int)Error.Response_HeadError;
                return null;
            }
        }
    }
}
