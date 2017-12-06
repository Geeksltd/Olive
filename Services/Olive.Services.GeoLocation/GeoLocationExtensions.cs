namespace Olive.Services.GeoLocation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GeoLocationExtensions
    {
        const int EARTH_RADIUS = 3963;

        /// <summary>
        /// Gets the geo distance in miles between this and another specified location.
        /// </summary>
        public static double? GetDistance(this IGeoLocation from, IGeoLocation to)
        {
            if (from == null) return null;

            if (to == null) return null;

            var dLat = (to.Latitude - from.Latitude).ToRadians();
            var dLon = (to.Longitude - from.Longitude).ToRadians();

            var a1 = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(from.Latitude.ToRadians()) * Math.Cos(to.Latitude.ToRadians()) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c1 = 2 * Math.Atan2(Math.Sqrt(a1), Math.Sqrt(1 - a1));

            var result = EARTH_RADIUS * c1;

            if (result > 100) return result.Round(0);
            else return result.Round(1);
        }

        /// <summary>
        /// Gets the geo distance in miles between this located object and a specified location.
        /// </summary>
        public static double? GetDistance(this IGeoLocated from, IGeoLocation to) => GetDistance(from.Get(l => l.GetLocation()), to);

        /// <summary>
        /// Gets the geo distance in miles between this location and a specified located object.
        /// </summary>
        public static double? GetDistance(this IGeoLocation from, IGeoLocated to) => GetDistance(from, to.Get(l => l.GetLocation()));

        /// <summary>
        /// Gets the geo distance in miles between this and another specified located object.
        /// </summary>
        public static double? GetDistance(this IGeoLocated from, IGeoLocated to) =>
            GetDistance(from.Get(l => l.GetLocation()), to.Get(l => l.GetLocation()));
    }
}