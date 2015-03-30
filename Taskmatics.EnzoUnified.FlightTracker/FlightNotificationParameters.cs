using System.ComponentModel.DataAnnotations;

namespace Taskmatics.EnzoUnified.FlightTracker
{
    public class FlightNotificationParameters
    {
        [Required]
        [Display(
            Name = "Enzo Unified ConnectionString", 
            Description = "The .NET connection string to use to connect to the Enzo Unified Datasource.",
            Order = 1)]
        public string EnzoConnectionString { get; set; }

        [Required]
        [Display(
            Name = "Airport Code",
            Description = "The ICAO airport code for the airport to be polled for new arriving flights.",
            Order = 2)]
        [RegularExpression("[A-Za-z]{4}")]
        public string AirportCode { get; set; }

        [Display(
            Name = "Flight Limiter",
            Description = "The maximum flight records to pull per request. Defaults to 10 if unspecified.",
            Order = 3)]
        [RegularExpression(@"\d*")]
        public int? RecordLimiter { get; set; }

        [Required]
        [Display(
            Name = "Phone Number(s)",
            Description = "One or more phone numbers that will receive new arrival information via SMS. Separate multiple numbers with a semicolon ';'.",
            Order = 4)]
        public string MobileNumber { get; set; }
    }
}
