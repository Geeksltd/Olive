namespace Zebble
{
    using System;
    using Olive;
    using Olive.GeoLocation;

    public class RadialRegion
    {
        const double EarthRadiusKm = 6371;
        const double EarthCircumferenceKm = EarthRadiusKm * 2 * Math.PI;
        const double MinimumRangeDegrees = 0.001 / EarthCircumferenceKm * 360; // 1 meter
        public RadialRegion(GeoLocation topLeft, GeoLocation bottomLeft, GeoLocation bottomRight)
        {
            TopLeft = topLeft;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Center = new GeoLocation(topLeft.Latitude - bottomRight.Latitude, topLeft.Longitude - bottomRight.Longitude);
            TopRight = new GeoLocation(Center.Latitude + (Center.Latitude - bottomLeft.Latitude), Center.Longitude + (Center.Longitude - bottomLeft.Longitude));
        }

        public GeoLocation TopLeft { get; }
        public GeoLocation TopRight { get; }
        public GeoLocation BottomLeft { get; }
        public GeoLocation BottomRight { get; }
        public GeoLocation Center { get; }
        public double LatitudeDegrees { get; }
        public double LongitudeDegrees { get; }

        public bool IsNorthup => TopLeft.Latitude > BottomLeft.Latitude && TopLeft.Longitude == BottomLeft.Longitude;

        public Distance Radius
        {
            get
            {
                var latKm = LatitudeDegreesToKm(LatitudeDegrees);
                var longKm = LongitudeDegreesToKm(Center, LongitudeDegrees);
                return new Distance(1000 * Math.Min(latKm, longKm) / 2);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RadialRegion s && Equals(s);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Center.GetHashCode();
                hashCode = (hashCode * 397) ^ LongitudeDegrees.GetHashCode();
                hashCode = (hashCode * 397) ^ LatitudeDegrees.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RadialRegion left, RadialRegion right) => Equals(left, right);

        public static bool operator !=(RadialRegion left, RadialRegion right) => !Equals(left, right);

        static double DistanceToLatitudeDegrees(Distance distance) => distance.Kilometers / EarthCircumferenceKm * 360;

        static double DistanceToLongitudeDegrees(GeoLocation position, Distance distance)
        {
            var latCircumference = LatitudeCircumferenceKm(position);
            return distance.Kilometers / latCircumference * 360;
        }

        bool Equals(RadialRegion other)
        {
            return Center.Equals(other.Center) && LongitudeDegrees.Equals(other.LongitudeDegrees) && LatitudeDegrees.Equals(other.LatitudeDegrees);
        }

        static double LatitudeCircumferenceKm(GeoLocation position)
        {
            return EarthCircumferenceKm * Math.Cos(position.Latitude.ToRadians());
        }

        public static double LatitudeDegreesToKm(double latitudeDegrees) => EarthCircumferenceKm * latitudeDegrees / 360;
        public static double LongitudeDegreesToKm(GeoLocation position, double longitudeDegrees)
        {
            var latCircumference = LatitudeCircumferenceKm(position);
            return latCircumference * longitudeDegrees / 360;
        }
    }
}