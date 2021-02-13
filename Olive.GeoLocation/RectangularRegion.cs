namespace Zebble
{
    using System.Linq;
    using Olive.GeoLocation;

    public class RectangularRegion
    {
        public GeoLocation TopLeft, BottomRight;

        public GeoLocation Centre()
        {
            return new GeoLocation
            {
                Latitude = new[] { TopLeft.Latitude, BottomRight.Latitude }.Average(),
                Longitude = new[] { TopLeft.Longitude, BottomRight.Longitude }.Average()
            };
        }

        public static RectangularRegion FromCentre(GeoLocation centre, double latitudeDelta, double longitudeDelta)
        {
            return new RectangularRegion
            {
                TopLeft = new GeoLocation
                {
                    Latitude = centre.Latitude - latitudeDelta / 2,
                    Longitude = centre.Longitude - longitudeDelta / 2
                },

                BottomRight = new GeoLocation
                {
                    Latitude = centre.Latitude + latitudeDelta / 2,
                    Longitude = centre.Longitude + longitudeDelta / 2
                }
            };
        }
    }
}
