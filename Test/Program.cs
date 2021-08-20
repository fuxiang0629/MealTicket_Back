using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionClient cache = new SessionClient();
            int error=cache.Connect("http://localhost:8800", "u1", "p1");
            error = cache.Set<A>("k1", new A { V = "v1" });
            var result=cache.Get<A>("k1", ref error);
        }
    }

    public class A 
    {
        public string V { get; set; }
    }
}
