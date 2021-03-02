using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTransactionDataUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            var SingletonInstance = Singleton.Instance;
            var MQHandlerInstance = MQHandler.instance;

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
