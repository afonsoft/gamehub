# https://aspnetboilerplate.com/Pages/Documents/Timing

## Timing

|     |     |     |
| --- | --- | --- |
| |     |     |
| --- | --- |
|  | × | | search |  |

Custom Search

|     |     |
| --- | --- |
|  | Sort by<br>Relevance<br>Date |

Version

latest (v10.4)v10.3v10.2v10.1v10.0v9.4.2v9.4.1v9.4.0v9.3.0v9.2.0v9.1.3v9.1v9.0v8.4v8.3v8.2v8.1v8.0v7.4v7.3v7.2v7.1v7.0-rc1v7.0v6.6.1v6.6.0v6.5.0v6.4-rc1v6.4.0v6.3.1v6.3v6.2v6.1.1v6.1.0v6.0v5.14v5.13v5.12v5.10.1v5.10v5.9v5.8v5.7v5.6v5.5v5.4v5.3v5.2v5.1.0v5.0.0v4.21v4.20v4.19v4.18v4.17v4.16v4.15v4.14v4.13v4.12v4.11.0v4.10.1v4.10.0v4.9.0v4.8.1v4.8.0v4.7.0v4.6.0v4.5.0v4.4.0v4.3.0v4.2.0v4.1.0v4.0.2v4.0.1v4.0.0v3.9.0v3.8.3v3.8.2v3.8.1v3.8.0v3.7.2v3.7.1v3.7.0v3.6.2v3.6.1v3.6.0v3.5.0v3.4.0v3.3.0v3.2.5v3.2.4v3.2.3v3.2.2v3.2.1v3.2.0v3.1.2v3.1.1v3.1.0v3.0.0-beta3v3.0.0-rc2v3.0.0-beta2v3.0.0-rc1v3.0.0-beta1v3.0.0v2.3.0v2.2.2v2.2.1v2.2.0v2.1.3v2.1.2v2.1.1v2.1.0-beta4v2.1.0-beta3v2.1.0-beta2v2.1.0-beta1v2.1.0v2.0.2v2.0.1v2.0.0-preview4v2.0.0-rc3v2.0.0-preview3v2.0.0-rc2v2.0.0-preview1v2.0.0v2.0.0-rcv1.5.2v1.5.1v1.5.0v1.4.3v1.4.2v1.4.1v1.4.0.0v1.3.1.0v1.3.0.0v1.2.2.0v1.2.1.0v1.2.0.0v1.1.3.0v1.1.1.0v1.1.0.0v1.0.0.0v0.10.3.2Menu

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Timing.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Timing\#introduction)

While some applications target a single timezone, others target
many different timezones. To satisfy such needs and centralize datetime
operations, ABP provides a common infrastructure for datetime operations.

### Clock [Anchor](https://aspnetboilerplate.com/Pages/Documents/Timing\#clock)

**Clock** is the main class used to work with **DateTime** values. It
defines the following **static** properties and methods:

- **Now**: Gets the current time according to the current provider.
- **Kind**: Gets the DateTimeKind of the current provider.
- **SupportsMultipleTimezone**: Gets a value that indicates whether or not a current
provider can be used for applications that need multiple timezones.
- **Normalize**: Normalizes/converts a given DateTime using the current
provider.

So, instead of using _DateTime.Now_, we use **Clock.Now**, which
abstracts it:

Copy

```
DateTime now = Clock.Now;
```

Clock uses the clock providers inside it. There are three types of
**built-in clock providers**:

- **ClockProviders.Unspecified** (UnspecifiedClockProvider): This is
the **default** clock provider and behaves just like
**DateTime.Now**. It acts as if you don't use the Clock class at all.
- **ClockProviders.Utc** (UtcClockProvider): Works in UTC datetime.
**DateTime.UtcNow** for Clock.Now. The Normalize method converts a given
datetime to a utc datetime and then sets it's kind to DateTimeKind.UTC. It
**supports multiple timezones**.
- **ClockProviders.Local** (LocalClockProvider): Works in the local
computer's time. The Normalize method converts a given datetime to a local
datetime and sets it's kind to DateTimeKind.Local.

You can set Clock.Provider in order to use a different clock provider:

Copy

```
Clock.Provider = ClockProviders.Utc;
```

This is generally done at the beginning of an application (do
it in the Application\_Start of a web application).

#### Client-Side [Anchor](https://aspnetboilerplate.com/Pages/Documents/Timing\#client-side)

The clock can be used on the client-side using the **abp.clock** object in
JavaScript. When you set Clock.Provider on the server-side, ABP
automatically sets the value of **abp.clock.provider** on the client-side.

### Time Zones [Anchor](https://aspnetboilerplate.com/Pages/Documents/Timing\#time-zones)

ABP defines a [setting](https://aspnetboilerplate.com/Pages/Documents/Setting-Management) named
**Abp.Timing.TimeZone** ( _TimingSettingNames.TimeZone_ constant) for
storing the selected timezone of the host, tenant and user. ABP assumes that
the value of a timezone setting is a valid **Windows timezone name**. It also
defines a timezone mapping file to convert a Windows Timezone to an
**IANA** timezone since some common libraries are using the IANA
timezone id. **UtcClockProvider** must be used in order to support
**multiple timezones**. Because if UtcClockProvider is used, all
datetime values will be stored in UTC and all datetimes will be sent to
clients in UTC format. Then on the client-side we can convert a UTC
datetime to the user's timezone by using the user's current timezone setting.

#### Client-Side [Anchor](https://aspnetboilerplate.com/Pages/Documents/Timing\#client-side-1)

ABP creates a JavaScript object named **abp.timing.timeZoneInfo** which
contains timezone information for the current user. This information
contains Windows and IANA timezone ids and some extra information for
windows timezone info. This information can be used to make client-side
datetime convertions by showing a datetime to the user in his/her timezone.

### Binders and Converters [Anchor](https://aspnetboilerplate.com/Pages/Documents/Timing\#binders-and-converters)

- ABP automatically normalizes DateTimes received (in an input class) from **clients** in
MVC, Web API and ASP.NET Core applications, based on the current
clock provider.
- ABP automatically normalizes DateTimes received from the **database**
based on the current clock provider, when
[EntityFramework](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration) or
[NHibernate](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration) modules are used.

If the UTC clock provider is used, then all DateTimes stored in the database are
assumed as UTC values, and all DateTimes received from clients are assumed
as UTC values unless explicitly specified.

### Disable DateTime Normalization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Timing\#disable-datetime-normalization)

In some cases, you might want ABP not to normalize your DateTime values. **DisableDateTimeNormalizationAttribute** is designed for such cases. This attribute can be used on classes, properties of classes or parameters of Controller Actions. when **DisableDateTimeNormalizationAttribute** is used, ABP will not normalize DateTime values.

Copy

```csharp
[DisableDateTimeNormalization]
public class Location : IHasCreationTime
{
	public string Lat { get; set; }

	public string Lng { get; set; }

	public DateTime CreationTime { get; set; }
}

public class Location : IHasCreationTime
{
	public string Lat { get; set; }

	public string Lng { get; set; }

	[DisableDateTimeNormalization]
	public DateTime CreationTime { get; set; }
}

[HttpGet]
public DateModel GetDateTimeKindProperty([DisableDateTimeNormalization]DateTime date)
{
	return new SimpleDateModel
	{
		Date = date
	};
}
```

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe