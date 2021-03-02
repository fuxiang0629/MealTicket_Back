using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Config 的摘要说明
/// </summary>
public class ConfigHandler : Handler
{
    public override async Task<string> Process()
    {
        return JsonConvert.SerializeObject(Config.Items);
    }
}