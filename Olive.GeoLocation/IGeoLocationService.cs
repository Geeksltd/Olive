using System.Threading.Tasks;

namespace Olive.GeoLocation
{
    public interface IGeoLocationService
    {
        /// <summary>
        ///  Gets the Geo Location of a specified postcode using Google API.
        ///  This method has daily usage limit of 25000 calls.
        /// </summary>
        Task<GeoLocation> GetPostcodeLocation(string postcode, string countryCode = "GB");

        /// <summary>
        /// Gets the distance between 2 locations in miles.
        /// </summary>
        Task<double?> CalculateDistance(string postcode1, string postcode2, string countryCode = "GB");

        /// <summary>
        /// Returns the traveling distance in miles using the Google Maps API.
        /// </summary>
        Task<double?> CalculateTravelDistance(string fromPostCode, string toPostCode, string countryCode = "GB");
    }
}
