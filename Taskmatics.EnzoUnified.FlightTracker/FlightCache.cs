using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Taskmatics.EnzoUnified.FlightTracker
{
    public class FlightCache
    {
        private static readonly string _cacheFilePath;
        private static readonly FlightComparer _flightComparer;
        private static readonly BinaryFormatter _formatter;

        static FlightCache()
        {
            _cacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "fltcache.dat");
            _flightComparer = new FlightComparer();
            _formatter = new BinaryFormatter();
        }

        public static List<ArrivedFlightInfo> GetCachedFlights()
        {
            var cachedArrivals = new List<ArrivedFlightInfo>();

            // Get cached arrivals, if any.
            if (File.Exists(_cacheFilePath))
                using (var cacheStream = File.Open(_cacheFilePath, FileMode.Open))
                    if (cacheStream.Length > 0)
                    {
                        var obj = _formatter.Deserialize(cacheStream);
                        if (obj != null && obj is List<ArrivedFlightInfo>)
                            cachedArrivals = obj as List<ArrivedFlightInfo>;
                    }

            return cachedArrivals;
        }

        public static void SaveFlightsToCache(List<ArrivedFlightInfo> flightsToCache)
        {
            var cachedFlights = GetCachedFlights();
            cachedFlights.AddRange(flightsToCache.Except(cachedFlights, _flightComparer));

            // Get rid of flights over 1 full day old.
            cachedFlights.RemoveAll(flight => flight.ArrivalDate.HasValue && flight.ArrivalDate < DateTime.Now.AddDays(-1).ToUniversalTime());

            // Save the cache back out to the file.
            using (var cacheStream = File.Open(_cacheFilePath, FileMode.Create))
                _formatter.Serialize(cacheStream, cachedFlights);
        }

        public static List<ArrivedFlightInfo> FilterNewArrivals(List<ArrivedFlightInfo> arrivalsToFilter)
        {
            var cachedArrivals = GetCachedFlights();

            // Compare cached arrivals to current retrieved arrivals to find flight arrivals that have not been sent yet.
            return arrivalsToFilter.Where(flight => !cachedArrivals.Contains(flight, _flightComparer)).ToList();
        }

        public class FlightComparer : IEqualityComparer<ArrivedFlightInfo>
        {
            public bool Equals(ArrivedFlightInfo x, ArrivedFlightInfo y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                return
                    x.Ident == y.Ident &&
                    x.AircraftType == y.AircraftType &&
                    x.ArrivalDate == y.ArrivalDate;
            }

            public int GetHashCode(ArrivedFlightInfo obj)
            {
                return String.Format("{0}|{1}|{2}",
                    obj.Ident, obj.AircraftType,
                    obj.ArrivalDate.HasValue ?
                        obj.ArrivalDate.Value.ToString("MMddyyyyHHmmssfff") :
                        DateTime.MinValue.ToString("MMddyyyyHHmmssfff"))
                    .GetHashCode();
            }
        }
    }
}
