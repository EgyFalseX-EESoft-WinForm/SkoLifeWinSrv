using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkoLifeWinSrv.BO;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskManager.DefaultInstance = new TaskManager();
            TaskManager.DefaultInstance.GetTasks();
            TaskManager.DefaultInstance.Start();
            Console.ReadLine();
        }
    }
}
