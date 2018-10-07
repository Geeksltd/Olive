# Olive.GeoLocation

This library provides these features:
1. Gets the Geo Location of a specified postcode using Google API
2. Gets the distance between 2 locations in miles
3. Returns the traveling distance in miles using the Google Maps API

## Getting started

First, you need to add the [Olive.GeoLocation](https://www.nuget.org/packages/Olive.GeoLocation/) NuGet package : `Install-Package Olive.GeoLocation`.

Olive exposes `IGeoLocationService` interface under `Olive.GeoLocation` namespace, which provide you with Geolocation functionality.

After adding nuget package, open `Startup.cs` file and in the `ConfigureServices(...)` method add `services.AddGeoLocationService();`. it should be something like this:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddGeoLocationService();
}
```

Now you should open `appsettings.json` and add this section:
```json
{
  [...]
  "Google.Maps.Api.Client.Key": "...",
  "Google.Maps.Api.Signature": "..."
  [...]
}

```

### Using GeoLocation

In order to use GeoLocation service, you should simply inject `IGeoLocationService` in your class constructor or use `Context.Current.GetService<Olive.GeoLocation.IGeoLocationService>();` if you want to have property injection as shown below:

```csharp
using Olive.GeoLocation;

namespace Domain
{
    public partial class ActionRequest
    {
        readonly IGeoLocationService GeoLocationService;

        public ActionRequest(IGeoLocationService geoLocationService)
        {
            this.GeoLocationService = geoLocationService;
        }

		[...]
    }
}
```

```csharp
async void GetPostCode()
{

    var postcodeLocation = GeoLocationService.GetPostcodeLocation("post code","GB");
}
```