# Custom SQL Data Type

You may face with scenarios which you need custom SQL data types or built-in types which are not supported by Olive as a default type. It's possible to achieve that using the following steps.

##### Set the database and C# type of your property.

```c#
String("Geography")
	.CSharpTypeName("SqlGeoLocationHelper.GeoGraphy")
	.DatabaseType("geography")
```

##### Create a value converter as below.
 > Make sure this class is `public` and `new-able` without any arguments.
```c#
public class GeoGraphyConverter : IValueConverter<GeoGraphy>
{
    public object ConvertFrom(GeoGraphy geoLocation)
    {
        if (geoLocation != null)
            return SqlGeography.Point((double)geoLocation?.Latitude, (double)geoLocation?.Longitude, Srid).STAsText();
        else
            return null;
    }

    public GeoGraphy ConvertTo(object value)
    {
        if (value == null || value == DBNull.Value)
            return new GeoGraphy();

        var geo = (SqlGeography)value;

        return !geo.IsNull ? GeoGraphy.Parse(geo) : new GeoGraphy();
    }
}
```

##### Mark the property with `CustomDataConverterAttribute` to let Olive use the correct converter.


```c#
String("Geography")
    .CSharpTypeName("SqlGeoLocationHelper.GeoGraphy")
    .DatabaseType("geography")
    .Attributes("[CustomDataConverter(\"Domain.GeoGraphyConverter\")]")
```
