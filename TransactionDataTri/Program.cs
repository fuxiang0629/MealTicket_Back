using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionDataTri
{
    class Program
    {
        static void Main(string[] args)
        {
            var WaitQueryInstance = WaitQuery.Instance;
            var mqHandler = WaitQueryInstance.StartMqHandler("TransactionDataTri");
            mqHandler.StartListen();
            Console.ReadLine();
        }
    }
}
