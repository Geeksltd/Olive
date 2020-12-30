using System;
using System.Linq;

namespace Olive.GeoLocation
{
    public interface IGeoLocation
    {
        double Longitude { get; }
        double Latitude { get; }
    }

    public class GeoLocation : IGeoLocation
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        
        public GeoLocation() { }

        public GeoLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString() => Latitude + ", " + Longitude;

        public static GeoLocation Parse(string latCommaLong)
        {
            var parts = latCommaLong.OrEmpty().Split(',').Trim().Select(x => x.TryParseAs<double>())
                .ExceptNull().ToArray();

            if (parts.Length != 2) throw new ArgumentException("GeoLocation.Parse() expects input format similar to 51.012, -3.0 ");

            return new GeoLocation { Latitude = parts.First(), Longitude = parts.Last() };
        }

        public static implicit operator GeoLocation(string text) => Parse(text);
    }
}
