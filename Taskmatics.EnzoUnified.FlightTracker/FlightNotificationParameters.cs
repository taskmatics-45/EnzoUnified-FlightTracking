using System.ComponentModel.DataAnnotations;

namespace Taskmatics.EnzoUnified.FlightTracker
{
    public class FlightNotificationParameters
    {
        [Required]
        public string EnzoConnectionString { get; set; }

        [Required]
        public string AirportCode { get; set; }

        public int? RecordLimiter { get; set; }

        [Required]
        public string MobileNumber { get; set; }
    }
}
