# IGeoLocation & IGeoLocated

This tutorial focuses on two important interfaces defined in Olive framework to calculate geo location. We often need to display geo location related features on our websites e.g. showing search results of properties in within 2Km distance of a postcode. We use latitude and longitude properties of a given address to calculate distances between two locations, which requires complex mathematical calculations. This tutorial explains Olive framework interfaces and classes implemented to manipulate geo location information.

## IGeoLocation Interface

This interface provided under `Olive.Services` namespace and defines on two basic properties required to maintain geo location i.e. Latitude and Longitude

```csharp
public interface IGeoLocation
{
    double Longitude { get; }
    double Latitude { get; }
}
```

## IGeoLocated Interface

This interface is also provided under `Olive.Services` namespace and only contains definition of one method “GetLocation()”. This method returns an “IGeoLocation” type object, which we discussed above.

```csharp
public interface IGeoLocated
{
    IGeoLocation GetLocation();
}
```

## Geo Location Extensions

Olive exposes extension methods on the interface explained above to perform distance calculation. An object of `Double?` is returned which contains the distance between provided geo locations. Below are the definitions with description of each extension method:

```csharp
/// <summary>
/// Gets the geo distance in miles between this and another specified location.
/// </summary>
public static double? GetDistance(this IGeoLocation from, IGeoLocation to)

/// <summary>
/// Gets the geo distance in miles between this located object and a specified location.
/// </summary>
public static double? GetDistance(this IGeoLocated from, IGeoLocation to)

/// <summary>
/// Gets the geo distance in miles between this location and a specified located object.
/// </summary>
public static double? GetDistance(this IGeoLocation from, IGeoLocated to)

/// <summary>
/// Gets the geo distance in miles between this and another specified located object.
/// </summary>
public static double? GetDistance(this IGeoLocated from, IGeoLocated to)
```