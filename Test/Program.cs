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
        static long accountId = 10000;
        static void Main(string[] args)
        {
            for (int j = 0; j < 100; j++)
            {
                Task[] tArr = new Task[5];
                for (int i = 0; i < 5; i++)
                {
                    tArr[i].Start();
                }
                Task.WaitAll(tArr);
                accountId++;
            }
            Console.ReadLine();
        }
    }
}
