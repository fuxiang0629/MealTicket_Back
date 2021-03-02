using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;


/// <summary>
/// Handler 的摘要说明
/// </summary>
public abstract class Handler
{
    public abstract Task<string> Process();
}