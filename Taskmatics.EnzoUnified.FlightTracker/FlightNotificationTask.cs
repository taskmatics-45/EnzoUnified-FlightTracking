using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Taskmatics.Scheduler.Core;

namespace Taskmatics.EnzoUnified.FlightTracker
{
    [InputParameters(typeof(FlightNotificationParameters))]
    public class FlightNotificationTask : TaskBase
    {
        protected override void Execute()
        {
            var parameters = (FlightNotificationParameters)Context.Parameters;

            // Get the recently arrived flights to the configured airport code.
            var arrivedFlights = GetArrivedFlights(parameters);

            // Compare with cached data.
            var newFlights = FlightCache.FilterNewArrivals(arrivedFlights);

            // Send the newly arrived flights to the SMS number configured.
            if (newFlights.Count > 0)
            {
                var results = SendArrivedFlightsViaSMS(newFlights, parameters);

                // If the message goes out successfully, update the cache so they won't go out again next time.
                if (results.All(result => String.IsNullOrWhiteSpace(result.ErrorCode)))
                    FlightCache.SaveFlightsToCache(newFlights);
            }
        }

        private List<ArrivedFlightInfo> GetArrivedFlights(FlightNotificationParameters parameters)
        {
            var results = new List<ArrivedFlightInfo>();

            using (var connection = new SqlConnection(parameters.EnzoConnectionString))
            using (var command = new SqlCommand("flightaware.arrived", connection))
            {
                connection.Open();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("airport", parameters.AirportCode));
                command.Parameters.Add(new SqlParameter("count", parameters.RecordLimiter ?? 10));
                command.Parameters.Add(new SqlParameter("type", "airline"));

                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        results.Add(new ArrivedFlightInfo
                        {
                            Ident = (String)reader["ident"],
                            AircraftType = (String)reader["aircrafttype"],
                            OriginICAO = (String)reader["origin"],
                            OriginName = (String)reader["originName"],
                            DepartureDate = reader["actualdeparturetime"] == DBNull.Value ? null : GetUTCDateFromUnixDateString((String)reader["actualdeparturetime"]),
                            ArrivalDate = reader["actualarrivaltime"] == DBNull.Value ? null : GetUTCDateFromUnixDateString((String)reader["actualarrivaltime"]),
                            DestinationICAO = (String)reader["destination"],
                            DestinationName = (String)reader["destinationName"],
                            DestinationCity = (String)reader["destinationCity"]
                        });
            }

            return results;
        }

        private List<SendSmsResult> SendArrivedFlightsViaSMS(List<ArrivedFlightInfo> flights, FlightNotificationParameters parameters)
        {
            var results = new List<SendSmsResult>();
            var smsMessage = GenerateSmsMessage(flights);

            using (var connection = new SqlConnection(parameters.EnzoConnectionString))
            using (var command = new SqlCommand("twilio.sendsms", connection))
            {
                connection.Open();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("phones", parameters.MobileNumber));
                command.Parameters.Add(new SqlParameter("message", smsMessage));

                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        results.Add(new SendSmsResult
                        {
                            MessageSid = (String)reader["Sid"],
                            AccountSid = (String)reader["AccountSid"],
                            ToPhoneNumber = (String)reader["To"],
                            FromPhoneNumber = (String)reader["From"],
                            Status = (String)reader["Status"],
                            SegmentCount = (String)reader["NumSegments"],
                            SentDate = reader["DateSent"] == DBNull.Value ? null : (DateTime?)reader["DateSent"],
                            Price = (decimal)reader["Price"],
                            Uri = new Uri((String)reader["Uri"]),
                            Direction = (String)reader["Direction"],
                            ErrorCode = (String)reader["ErrorCode"],
                            ErrorMessage = (String)reader["ErrorMessage"]
                        });
            }

            return results;
        }

        private DateTime? GetUTCDateFromUnixDateString(String seconds)
        {
            long secondsToAdd = 0L;
            if (long.TryParse(seconds, out secondsToAdd))
                return (DateTime?)new DateTime(1970, 1, 1).AddSeconds(secondsToAdd).ToUniversalTime();

            return null;
        }

        private string GenerateSmsMessage(List<ArrivedFlightInfo> flights)
        {
            var firstFlight = flights.First();
            var destinationInfo = String.Format("{0}, {1}", firstFlight.DestinationName, firstFlight.DestinationCity);
            var flightInfo = new StringBuilder();
            foreach (var flight in flights)
                flightInfo.AppendFormat("{0}\r\n", flight);

            return String.Format("New Arrivals to {0}:\r\n{1}",
                destinationInfo,
                flightInfo);
        }
    }
}
