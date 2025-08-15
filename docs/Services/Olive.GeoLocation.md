# Olive GeoLocation Documentation

## Overview

Olive GeoLocation provides robust functionality for handling geographical data and calculations. It simplifies operations such as retrieving geographic coordinates, calculating distances between locations, and working with different geographic regions. Leveraging Olive's integration, it seamlessly interacts with Google Maps APIs for accurate geospatial calculations.

---

## Table of Contents

- [Key Functionalities](#key-functionalities)
- [Installation and Setup](#getting-started)
- [Core Components](#core-components)
  - [GeoLocation Class](#geolocation-class)
  - [Distance Struct](#distance-struct)
  - [GeoLocationService](#geolocationservice)
  - [RadialRegion and RectangularRegion](#radialregion-and-rectangularregion-classes)
- [Configuration](#configuration)
- [Usage Examples](#usage-examples)
- [Dependencies](#dependencies)
- [Exception Handling and Logging](#exception-handling-and-logging)
- [Important Notes](#important-notes)

---

## Key Functionalities

- **Determining Geo Coordinates:** Get longitude and latitude for locations and postcodes using Google's Geocoding API.
- **Distance Calculations:** Compute geographic distances (straight-line) and travel distances between points.
- **Geographical Regions:** Defining and using specific rectangular and radial regions for geographic analyses.

---

## Getting started

First, install the [Olive.GeoLocation](https://www.nuget.org/packages/Olive.GeoLocation/) NuGet package into your project:

```
Install-Package Olive.GeoLocation
```

Then register the GeoLocation services using dependency injection in your application (`Startup.cs` file):

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddGeoLocationService();
}
```

---

## Key Functionalities

### GeoLocation Class

The core representation of geographic points with latitude and longitude coordinates.

Properties:
- **Latitude** (`double`) — Latitude coordinate.
- **Longitude** (`double`) — Longitude coordinate.

### Parse Example:

```csharp
var location = GeoLocation.Parse("51.5074,-0.1278");
Console.WriteLine(location.Latitude); // Outputs: 51.5074
```

---

### Distance Struct

Represents distances and provides convenient conversions between meters, kilometers, and miles.

Properties:
- `Meters` — Total meters.
- `Kilometers` — Converted from meters.
- `Miles` — Converted from meters to miles.

Constructors:
- `FromMeters(double meters)`
- `FromMiles(double miles)`
- `FromKilometers(double kilometers)`

### Example Usage:

```csharp
var distance = Distance.FromMiles(10);
Console.WriteLine($"Kilometers: {distance.Kilometers} km"); // Outputs: "Kilometers: 16.093 km"
```

---

## RadialRegion and RectangularRegion Classes

Define geospatial regions used in geographical calculations or analyses:

### RadialRegion

Represents a radial (circular) geographic area defined by a central point and radius.

### RectangularRegion

Represents rectangular geographic areas by two opposite corners.

---

## Configuration

After installing the package, ensure you configure the Google Maps API integration properly by adding the following to your `appsettings.json` file:

```json
{
  "Google": {
    "Maps": {
      "Api": {
        "Client": {
          "Key": "[YOUR_GOOGLE_API_KEY]"
        },
        "Signature": "[YOUR_GOOGLE_API_SIGNATURE]" 
      }
    }
  }
}
```

_**Important**_: Obtain your API key from the [Google Cloud Console](https://console.cloud.google.com/).

---

## Using the GeoLocation Service

The GeoLocation service is configured via dependency injection and can be used to perform various geolocation calculations:

```csharp
public class ActionRequest
{
    readonly IGeoLocationService GeoLocationService;

    public ActionRequest(IGeoLocationService geoLocationService)
    {
        GeoLocationService = geoLocationService;
    }
}
```

### Methods available:

- **GetPostcodeLocation**: Retrieves latitude and longitude for a specified postcode.
- **CalculateDistance**: Calculates the straight-line distance (geographical miles) between two postcodes.
- **CalculateTravelDistance**: Computes actual travel distance between locations using Google Maps API.

---

## RadialRegion and RectangularRegion Classes

### RadialRegion

Defines a radial (circular) geographic area based on a center and radius.

```csharp
var center = new GeoLocation { Latitude = 51.5074, Longitude = -0.1278 };
var radialRegion = new RadialRegion(center, Distance.FromKilometers(10));
```

### RectangularRegion

Defines a rectangular area by specifying top-left and bottom-right coordinates.

```csharp
var region = new RectangularRegion
{
    TopLeft = new GeoLocation { Latitude = 50.0, Longitude = -10.0 },
    BottomRight = new GeoLocation { Latitude = 40.0, Longitude = 0.0 }
};

var midpoint = region.Centre(); // Center of the rectangle
```

---

## Configuration

Ensure these app configuration settings exist:

```json
{
  "Google": {
    "Maps": {
      "Api": {
        "Client": {
          "Key": "YOUR_API_KEY"
        },
        "Signature": "[Optional_Google_Signature]"
      }
    }
  }
}
```

---

## Usage Examples

### Calculating Geo-distance Between Locations

```csharp
var geo = new GeoLocationService();

var location1 = await geo.GetPostcodeLocation("E14 5AB");
var location2 = await geoLocationService.GetPostcodeLocation("W1A 1AA");

var distance = location1.GetDistance(location2); // Distance in miles
```

### Calculate Travel Distance (Route) Using Google Maps API:

```csharp
var travelDistance = await geoLocationService.CalculateTravelDistance("E14 5AB", "W1A 1AA");
Console.WriteLine($"Travel distance: {travelDistance} miles");
```

---

## Exception Handling and Logging

The GeoLocation library provides detailed error handling and logging mechanisms, including:

- Validation errors for invalid GeoLocation data
- Comprehensive exceptions detailing Google API errors
- Logs configuration mistakes, such as missing or invalid API keys

**Example logged exception:**

```text
Exception: Google API Error: REQUEST_DENIED
```

---

## Dependencies

- Olive core library and entities packages required:

```bash
Install-Package Olive
Install-Package Olive.Entities
```

- Configured Google Maps API key (with GEOCODE API access enabled).

---

## Important Notes

- Google API has usage limits (default daily limit of around 25,000 requests) unless extended with billing enabled.
- Verify your Google API quotas and monitor usage to prevent exceeding daily limits.
- GeoLocation representations might slightly vary depending on region data accuracy.
- Ensure security of geographic data (e.g., anonymized or authorized access only).  