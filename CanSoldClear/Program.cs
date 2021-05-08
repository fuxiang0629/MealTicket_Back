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
            RunnerHelper.JoinCanSold();//可售
            //long serviceFee = ((long)Math.Round(399878571 * (250 * 1.0 / 100000), 0)) / 100 * 100;
        }
    }
}
