using System;

namespace Taskmatics.EnzoUnified.FlightTracker
{
    [Serializable]
    public class ArrivedFlightInfo
    {
        public string Ident { get; set; }
        public string AircraftType { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string OriginICAO { get; set; }
        public string OriginName { get; set; }
        public string DestinationICAO { get; set; }
        public string DestinationName { get; set; }
        public string DestinationCity { get; set; }

        public override string ToString()
        {
            return String.Format("{0}: Arrived {1} UTC From {2}.",
                Ident,
                ArrivalDate.HasValue ? ArrivalDate.Value.ToString("MM/dd/yyyy hh:mm:ss tt") : "N/A",
                OriginICAO);
        }
    }

    public class SendSmsResult
    {
        public string MessageSid { get; set; }
        public string AccountSid { get; set; }
        public string ToPhoneNumber { get; set; }
        public string FromPhoneNumber { get; set; }
        public string Status { get; set; }
        public string SegmentCount { get; set; }
        public DateTime? SentDate { get; set; }
        public decimal Price { get; set; }
        public string Direction { get; set; }
        public Uri Uri { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
