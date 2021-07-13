using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanSoldClear
{
    class Program
    {
        static void Main(string[] args)
        {
            RunnerHelper.TradeClean();//清理
            RunnerHelper.JoinCanSold();//可售
        }
    }
}
