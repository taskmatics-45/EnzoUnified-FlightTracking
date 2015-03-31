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
                EnzoConnectionString = "Server=<your server here>; Database=<your DB name here>; Uid=<your uid>; Pwd=<your pwd>",
                AirportCode = "LAX",
                MobileNumber = "1235551212"
            };

            var harness = new TaskHarness<FlightNotificationTask>(parameters);
            harness.Execute();

            Console.WriteLine("done...");
            Console.ReadLine();
        }
    }
}
