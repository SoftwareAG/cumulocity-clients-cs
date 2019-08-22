## Developing REST clients

The section is tightly linked to the design of the REST interfaces, which are described in [REST implementation](https://cumulocity.com/guides/reference/rest-implementation) in the Reference guide.

Documentation is available on the [resources site](http://resources.cumulocity.com/documentation/cssdk/current/).

### Connecting to Cumulocity

The root interface for connecting to Cumulocity from C# is called "Platform" (see "Root interface" in [REST implementation](https://cumulocity.com/guides/reference/rest-implementation) in the Reference guide). It provides access to all other interfaces of the platform, such as the inventory. In its simplest form, it is instantiated as follows:

```cs
Platform platform = new PlatformImpl("<<URL>>", new CumulocityCredentials("<<user>>", "<<password>>"));
```

As an example:

```cs
Platform platform = new PlatformImpl("https://demos.cumulocity.com", new CumulocityCredentials("myuser", "mypassword"));
```

If you use the C# client for developing an application, you need to register an application key (through [Own applications](https://cumulocity.com/guides/users-guide/administration#managing-applications) in the Cumulocity Administration application, or through the [Application API](https://cumulocity.com/guides/reference/applications)).

```cs
new CumulocityCredentials("<<tenantID>>", "<<user>>", "<<password>>", "<<application key>>")
```

For testing purposes, every tenant is subscribed to the demo application key "uL27no8nhvLlYmW1JIK1CA==". The constructor for PlatformImpl also allows you to specify the default number of objects returned from the server in one reply with the parameter "pageSize".

### Accessing the inventory

The following code snippet shows how to obtain a handle to the inventory from C#:

```cs
IInventoryApi inventory = platform.InventoryApi;
```

Using this handle, you can create, retrieve and update managed objects. For example, if you would like to retrieve all objects that have a geographical position, use

```cs
InventoryFilter inventoryFilter = new InventoryFilter();
inventoryFilter.ByFragmentType(typeof(Position));
var moc = inventory.GetManagedObjectsByFilter(inventoryFilter);
```

This returns a query to get the objects -- it does not actually get them. In practice, such a list of objects could be very large. Hence, it is returned in "pages" from the server. To get all pages and iterate over them, use:

```cs
foreach (ManagedObjectRepresentation mo in moc.GetFirstPage().AllPages())
	{
		Console.WriteLine(mo.Id);
	}
```

To create a new managed object, simply construct a local representation of the object and send it to the platform. The following code snippet shows how to create a new electricity meter with a relay in it:

```cs
ManagedObjectRepresentation mo = new ManagedObjectRepresentation();
mo.Name = "MyMeter-1";
Relay relay = new Relay();
mo.Set(relay);
SinglePhaseElectricitySensor meter = new SinglePhaseElectricitySensor();
mo.Set(meter);
// Set additional properties, e.g., tariff tables, ...
mo = inventory.Create(mo);
Console.WriteLine(mo.Id);
```

The result of invoking "create" is a version of the new managed object with a populated unique identifier.

Now assume that you would like to store additional, own properties along with the device. This can simply be done by creating a new "fragment" in the form of a C# class. For example, assume that you would like to store tariff information along with your meter. There is a day and a night time tariff, and we need to store the hours during which the night time tariff is active:

```cs
[PackageName("tariff")]
public class Tariff
{
	[JsonProperty("nightTariffStart")]
	public int NightTariffStart
	{
		get
		{
			return nightTariffStart;
		}
		set
		{
			this.nightTariffStart = value;
		}
	}
	[JsonProperty("nightTariffEnd")]
	public int NightTariffEnd
	{
		get
		{
			return nightTariffEnd;
		}
		set
		{
			this.nightTariffEnd = value;
		}
	}
	private int nightTariffStart = 22;
	private int nightTariffEnd = 6;
}
```

Now, you can simply add tariff information to your meter:

```cs
Tariff tariff = new Tariff();
mo.Set(tariff);
```

This will store the tariff information along with the meter. For converting C# objects from and towards JSON/REST, Cumulocity uses Json.NET. The [Json.NET help](https://www.newtonsoft.com/json/help) provides more information on how to influence the JSON format that is produced respectively accepted.

### Accessing the identity service

A device typically has a technical identifier that an agent needs to know to be able to contact the device. Examples are meter numbers, IP addresses and REST URLs. To associate such identifiers with the unique identifier of Cumulocity, agents can use the identity service. Again, to create the association, create an object of type "ExternalIDRepresentation" and send it to the platform.

The code snippet below shows how to register a REST URL for a device. It assumes that "mo" is the managed object from the above example and "deviceUrl" is a string with the REST URL of the device.

```cs
const string ASSET_TYPE = "com_cumulocity_idtype_AssetTag";
const string deviceUrl = "SAMPLE-A-239239232";

ExternalIDRepresentation externalIDGid = new ExternalIDRepresentation();
externalIDGid.Type = ASSET_TYPE;
externalIDGid.ExternalId = deviceUrl;
externalIDGid.ManagedObject = mo;
IIdentityApi identityApi = platform.IdentityApi;
identityApi.Create(externalIDGid);
```

Now, if you need the association back, you can just query the identity service as follows:

```cs
ID id = new ID();
id.Type = ASSET_TYPE;
id.Value = deviceUrl;
externalIDGid = identityApi.GetExternalId(id);
```

The returned object will contain the unique identifier and a link to the managed object.

### Accessing events and measurements

Events and measurements can be accessed in a very similar manner as described above for the inventory. The following example queries the signal strength of the mobile connection of devices in the past two weeks and prints the device ID, the time of the measurement, the received signal strength and the bit error rate.

```cs
IMeasurementApi measurementApi = platform.MeasurementApi;
MeasurementFilter measurementFilter = new MeasurementFilter();

var toDate = DateTime.Now;
var fromDate = DateTime.Now.AddDays(-14);
measurementFilter.ByDate(fromDate, toDate);
measurementFilter.byFragmentType(typeof(SignalStrength));
IMeasurementCollection mc = measurementApi.GetMeasurementsByFilter(measurementFilter);

MeasurementCollectionRepresentation measurements = mc.GetFirstPage();

foreach (var measurement in mc.GetFirstPage().AllPages())
   {
       SignalStrength signal = measurement.Get<SignalStrength>();
       Console.WriteLine(measurement.Source.Id + " " + measurement.DateTime + " " + signal.RssiValue + " " + signal.BerValue);
   }
```

### Controlling devices

Finally, the "DeviceControlResource" enables you to manipulate devices remotely. It has two sides: You can create operations in applications to be sent to devices, and you can query operations from agents.

In order to control a device it must be in the "childDevices" hierarchy of an agent managed object. The agent managed object represents your agent in the inventory. It is identified by a fragment com\_cumulocity\_model\_Agent. This is how Cumulocity identifies where to send operations to control a particular device.

This code demonstrates the setup:

```cs
ManagedObjectRepresentation agent = new ManagedObjectRepresentation();
agent.Set(new Agent()); // agents must include this fragment
// ... create agent in inventory
ManagedObjectRepresentation device = ...;
// ... create device in inventory

ManagedObjectReferenceRepresentation child2Ref = new ManagedObjectReferenceRepresentation();
child2Ref.ManagedObject= device;
inventory.GetManagedObject(agent.Id).AddChildDevice(child2Ref);
```

For example, assume that you would like to switch off a relay in a meter from an application. Similar to the previous examples, you create the operation to be executed locally, and then send it to the platform:

```cs
IDeviceControlApi control = platform.DeviceControlApi;
OperationRepresentation operation = new OperationRepresentation
{
     DeviceId = mo.Id
};
relay.SetRelayState(Relay.RelayState.OPEN);
operation.Set(relay);
control.Create(operation);
```

Now, if you would like to query the pending operations from an agent, the following code would need to be executed:

```cs
OperationFilter operationFilter = new OperationFilter();
operationFilter.ByAgent(mo.Id.Value);
operationFilter.ByStatus(OperationStatus.PENDING);
IOperationCollection oc = deviceControlApi.GetOperationsByFilter(operationFilter);
```

Again, the returned result may come in several pages due to its potential size.

```cs
foreach (OperationRepresentation op in oc.GetFirstPage().AllPages())
{
	Console.WriteLine(op.Status);
}
```

### Realtime features

The C# client libraries fully support the real-time APIs of Cumulocity. For example, to get immediately notified when someone sends an operation to your agent, use the following code:

```cs
subscriber = new OperationNotificationSubscriber(platform);
subscriber.Subscribe(agentId, new Handler(operationProcessor));

public class Handler : ISubscriptionListener<GId, OperationRepresentation>
{
    private SimpleOperationProcessor operationProcessor;

    public Handler(SimpleOperationProcessor processor)
    {
        this.operationProcessor = processor;
    }

    public void OnError(ISubscription<GId> subscription, Exception ex)
    {
    }

    public void OnNotification(ISubscription<GId> subscription, OperationRepresentation notification)
    {
        operationProcessor.Process(notification);
    }
}
```

> **Info**: `"agentId"` is the ID of your agent in the inventory.

To unsubscribe from a subscription, use the following code:

```cs
subscription.Unsubscribe()
```

If you wish to disconnect, the following code must be used:

```cs
subscriber.Disconnect()
```

### Reliability features

In particular on mobile devices, Internet connectivity might be unreliable. To support such environments, the C# client libraries support local buffering. This means that you can pass data to the client libraries regardless of an Internet connection being available or not. If a connection is available, the data will be send immediately. If not, the data will be buffered until the connection is back again. For this, "async" variants of the API calls are offered. For example, to send an alarm, use

```cs
IAlarmApi alarmApi = platform.AlarmApi;
Task<AlarmRepresentation> task = alarmApi.CreateAsync(anAlarm);
```

The "createAsync" method returns immediately. The "Task" object can be used to determine the result of the request whenever it was actually carried out.
To ensure that messages are buffered in the event of a communication failure, set the a AsyncEnabled parameter to true in ClientConfiguration:

```cs
new ClientConfiguration(new MemoryBasedPersistentProvider(), true);
```

### Logging configuration

Logging in the C# client SDK is handled through [LibLog](https://github.com/damianh/LibLog/). For a detailed description on how to use and configure logging, see the [logback documentation](https://github.com/damianh/LibLog/wiki).

Since version 0.11, the default logging level of the SDK is set to "Error" for all components, which means that logging messages are suppressed unless their level is "Error". If everything runs smoothly, there should be no log messages generated by the SDK. By default, log messages are sent to the console only.

With liblog you actually embed a blob of code into the library. This code then picks up which logging abstraction is in use by the application and writes to it via some clever reflection code. It has transparent built-in support for the following logging providers:

* NLog
* Log4Net
* EntLib Logging
* Serilog
* Loupe
* Custom Provider

As soon as you add the configuration for your preferred provider and set up the necessary appender, logging should just work. If you wish to implement your custom provider, you just need to ensure that your provider implements the ILogProvider interface or inherits from the LogProviderBase.


#### Simple logging configuration

The following example shows how to enable debug-level logging for a single component, called "MyClass", whilst keeping error-level logging for all other components. The following code snippet shows how to create the logger, and to log a message:

```cs
public class MyClass
{
	private static readonly ILog Logger = LogProvider.For<MyClass>();

	public void DoSomething()
	{
		Logger.Debug("Method 'DoSomething' in progress");
	}
}
```

That’s it, now the library is ready to automatically pick-up logger used by the consuming application. For example, if Serilog is the selected library, assigning Serilog’s Logger.Log will automatically connect all the moving parts together:

```cs
Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Verbose()
             .WriteTo.LiterateConsole()
             .CreateLogger();

Log.Logger.Verbose("Starting...");

var myClass = new MyClass();
myClass.DoSomething();

Log.Logger.Verbose("Finishing...");
Console.ReadKey();
```

When the code is run, the console should contain a message similar to the following:

```bash
The result is

[21:09:18 APP] Starting...
[21:09:18 APP] Method 'DoSomething' in progress
[21:09:18 APP] Finishing...
```
