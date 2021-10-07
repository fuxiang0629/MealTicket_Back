using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.API
{
    public class GetSecurityCountApi: BaseApi
    {
        /// <summary>
        /// 解析返回结果
        /// </summary>
        /// <param name="response_body"></param>
        /// <returns></returns>
        internal int ParseResponse(byte[] response_body)
        {
            int count = BitConverter.ToUInt16(response_body, 0);
            return count;
        }

        /// <summary>
        /// 设置请求参数
        /// </summary>
        /// <param name="market"></param>
        /// <returns></returns>
        internal byte[] SetRequestParams(byte market)
        {
            return Utils.HexToByte(string.Format("0c 0c 18 6c 00 01 08 00 08 00 4e 04 0{0} 00 75 c7 33 01", market));
        }
    }
}
