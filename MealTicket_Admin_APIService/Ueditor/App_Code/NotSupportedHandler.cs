using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// NotSupportedHandler 的摘要说明
/// </summary>
public class NotSupportedHandler : Handler
{
    public override async Task<string> Process()
    {
        var temp = new
        {
            state = "action 参数为空或者 action 不被支持。"
        };
        return JsonConvert.SerializeObject(temp);
    }
}