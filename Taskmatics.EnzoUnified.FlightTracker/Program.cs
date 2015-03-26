using System;
using Taskmatics.Scheduler.Core;

namespace Taskmatics.EnzoUnified.FlightTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            var parameters = new FlightNotificationParameters
            {
                EnzoConnectionString = "Server=data.enzounified.com,9553; Database=BSC; Uid=taskmatics; Pwd=T@sk00!",
                AirportCode = "BDL",
                MobileNumber = "2038167590"
            };

            var harness = new TaskHarness<FlightNotificationTask>(parameters);
            harness.Execute();

            Console.WriteLine("done...");
            Console.ReadLine();
        }
    }
}
