using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Olive.GeoLocation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GeoLocationExtensions
    {
        const int EARTH_RADIUS = 3963;
        const double CIRCLE = 360;

        /// <summary>
        /// Gets the geo distance in miles between this and another specified location.
        /// </summary>
        public static double? GetDistance(this IGeoLocation @this, IGeoLocation to)
        {
            if (@this == null) return null;
            if (to == null) return null;

            var dLat = (to.Latitude - @this.Latitude).ToRadians();
            var dLon = (to.Longitude - @this.Longitude).ToRadians();

            var a1 = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(@this.Latitude.ToRadians()) * Math.Cos(to.Latitude.ToRadians()) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c1 = 2 * Math.Atan2(Math.Sqrt(a1), Math.Sqrt(1 - a1));

            var result = EARTH_RADIUS * c1;

            if (result > 100) return result.Round(0);
            else return result.Round(1);
        }

        /// <summary>
        /// Gets the geo distance in miles between this located object and a specified location.
        /// </summary>
        public static double? GetDistance(this IGeoLocated @this, IGeoLocation to) => GetDistance(@this?.GetLocation(), to);

        /// <summary>
        /// Gets the geo distance in miles between this location and a specified located object.
        /// </summary>
        public static double? GetDistance(this IGeoLocation @this, IGeoLocated to) => GetDistance(@this, to?.GetLocation());

        /// <summary>
        /// Gets the geo distance in miles between this and another specified located object.
        /// </summary>
        public static double? GetDistance(this IGeoLocated @this, IGeoLocated to) =>
            GetDistance(@this?.GetLocation(), to?.GetLocation());

        public static IServiceCollection AddGeoLocationService(this IServiceCollection @this)
        {
            return @this.AddSingleton<IGeoLocationService, GeoLocationService>();
        }

        public static double GetCompassAngle(this IGeoLocation from, IGeoLocation to)
        {
            var longitudeDifference = (to.Longitude - from.Longitude).ToRadians();

            var dPhi = Math.Log(Math.Tan(to.Latitude.ToRadians() / 2 + Math.PI / 4)
                                / Math.Tan(from.Latitude.ToRadians() / 2 + Math.PI / 4));

            if (Math.Abs(longitudeDifference) > Math.PI)
                longitudeDifference = longitudeDifference > 0 ? -(2 * Math.PI - longitudeDifference) : (2 * Math.PI + longitudeDifference);

            return (Math.Atan2(longitudeDifference, dPhi).ToDegreeFromRadians() + CIRCLE) % CIRCLE;
        }

        public static double ToDegreeFromRadians(this double radians) => radians * (180.0 / Math.PI);
    }
}