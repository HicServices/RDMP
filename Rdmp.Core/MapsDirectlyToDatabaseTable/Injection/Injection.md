# IInjectKnown&lt;T&gt;

## Background

In RDMP `DatabaseEntity` objects are class instances that map 1 to 1 with a row in a table in a database.  

* The table will have the same name as the class
* The public properties map to columns of the same name in the table (unless they are marked `[NoMappingToDatabase]`)
* There will be a public `ID` property on the class and an `ID` column which is identity autonum and primary key in the table

Often your class will have foreign key properties e.g. `CatalogueItem.Catalogue_ID`.  For these properties it is recommended to create a property `Catalogue` which returns the corresponding object.  For example

```csharp
 public Catalogue Catalogue 
 {
	get { return Repository.GetObjectByID<Catalogue>(Catalogue_ID); }
 }
```

Since this is a public property you will need to decorate it with `[NoMappingToDatabase]` so that RDMP doesn't think that there should be a corresponding property in the database table.

There are some drawbacks to this:

* Every time the programmer references the property they will go to the database
* Since a new instance is fetched each time you have to copy the returned value to a local variable to edit it properly
* Any UI code that references it e.g. ToString etc will cause lots of redundant database calls

# Enter IInjectKnown&lt;T&gt;
`IInjectKnown<T>` is a structured way of handling the caching of known answers as well as injecting known answers in anticipation of someone asking one day.

It is intended to be a wrapper around `Lazy<T>` that simplifies caching of these relationship properties as well as providing a single point for marking the cache invalid (e.g. when you change `CatalogueItem.Catalogue_ID` int property).

# Implementing IInjectKnown&lt;T&gt;

Consider the imaginary class skeleton `AggregateTopX`, this class will be dependent on a parent class `AggregateConfiguration`

```csharp
public class AggregateTopX: DatabaseEntity
{
	#region Database Properties

	private int _aggregateConfiguration_ID;
	
	#endregion

	public int AggregateConfiguration_ID
	{
		get { return _aggregateConfiguration_ID;}
		set { SetField(ref _aggregateConfiguration_ID, value);}
	}
	
	//constructor for creating new instances in memory/database simultaneously
	public AggregateTopX(IRepository repository,AggregateConfiguration config)
	{
		repository.InsertAndHydrate(this,new Dictionary<string, object>()
		{
			{"AggregateConfiguration",config.ID}
		});

		if (ID == 0 || Repository != repository)
			throw new ArgumentException("Repository failed to properly hydrate this class");
	}
	
	//internal constructor for creating instances out of the database
	internal AggregateTopX(IRepository repository, DbDataReader r): base(repository, r)
	{
		AggregateConfiguration_ID = Convert.ToInt32(r["AggregateConfiguration_ID"]);
	}
}
```

First create a relationship property with a getter that gets the parent object.  This will go to the database every time.

```csharp

    #region Relationships

	[NoMappingToDatabase]
    public AggregateConfiguration AggregateConfiguration
    {
        get { return Repository.GetObjectByID<AggregateConfiguration>(AggregateConfiguration_ID); }
    }

    #endregion
```

To cache this result we should move the call to a `Lazy<AggregateConfiguration>` field.  We can initialize this field in a method `SetupLazy`

```csharp

    #region Relationships

    Lazy<AggregateConfiguration> _knownAggregateConfiguration;

    [NoMappingToDatabase]
    public AggregateConfiguration AggregateConfiguration
    {
        get { return _knownAggregateConfiguration.Value; }
    }

    public void SetupLazy()
    {
        _knownAggregateConfiguration = new Lazy<AggregateConfiguration>(() => Repository.GetObjectByID<AggregateConfiguration>(AggregateConfiguration_ID));
    }
    #endregion
```

This is going to get out of hand fast, especially if we have multiple relationship properties etc.  It also isn't exposed by any kind of interfaces.  To deal with this we have the interface `IInjectKnown<T>`.

Implement the interface `IInjectKnown` for `AggregateConfiguration`.  Rename the `SetupLazy` method to `ClearAllInjections`.

```csharp
public class AggregateTopX : DatabaseEntity, IInjectKnown<AggregateConfiguration>
{
    #region Database Properties

    private int _aggregateConfiguration_ID;

    #endregion

    #region Relationships

    Lazy<AggregateConfiguration> _knownAggregateConfiguration;

    [NoMappingToDatabase]
    public AggregateConfiguration AggregateConfiguration
    {
        get { return _knownAggregateConfiguration.Value; }
    }

    #endregion
    
    public int AggregateConfiguration_ID
    {
        get { return _aggregateConfiguration_ID; }
        set { SetField(ref _aggregateConfiguration_ID, value); }
    }
    
    //Constructor that puts a new record into the database
    public AggregateTopX(IRepository repository, AggregateConfiguration config)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>()
		{
			{"AggregateConfiguration",config.ID}
		});

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");
    }

    //Constructor for fetching existing instances out of the database
    internal AggregateTopX(IRepository repository, DbDataReader r)
        : base(repository, r)
    {
        AggregateConfiguration_ID = Convert.ToInt32(r["AggregateConfiguration_ID"]);
    }

    public void InjectKnown(AggregateConfiguration instance)
    {
        throw new NotImplementedException();
    }

    public void ClearAllInjections()
    {
        _knownAggregateConfiguration = new Lazy<AggregateConfiguration>(() => Repository.GetObjectByID<AggregateConfiguration>(AggregateConfiguration_ID));
    }
}
```

Finally we need to handle `InjectKnown` and make sure that `ClearAllInjections`/`InjectKnown` gets called in our constructors correctly (otherwise we will get a null reference exception when someone uses the `AggregateConfiguration` property.

```csharp
public void InjectKnown(AggregateConfiguration instance)
    {
        _knownAggregateConfiguration = new Lazy<AggregateConfiguration>(()=>instance);
    }
```


```csharp
 //Constructor that puts a new record into the database
    public AggregateTopX(IRepository repository, AggregateConfiguration config)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>()
		{
			{"AggregateConfiguration",config.ID}
		});

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");

        ClearAllInjections();
    }

    //Constructor for fetching existing instances out of the database
    internal AggregateTopX(IRepository repository, DbDataReader r)
        : base(repository, r)
    {
        AggregateConfiguration_ID = Convert.ToInt32(r["AggregateConfiguration_ID"]);

        ClearAllInjections();
    }
```

# Advantages

Now we have an interface and standardised public methods we can do things like

* Fetch all `AggregateConfiguration` objects from the database as an array
* Fetch all `AggregateTopX` objects from the database as an array 
	* Foreach `AggregateTopX` call `InjectKnown` with the requisite `AggregateConfiguration` object which is already in memory

This means that we have 2 database calls up front and never have to make another one since we did a fast operation in memory to tell all the `AggregateTopX` what their associated `AggregateConfiguration` are.  This may not seem like must of a big deal but if you have thousands of objects it can really help prevent you from ddosing your metadata database.

Even if you never call `InjectKnown`, having a standard interface method for caching relationship property objects and invalidating these caches (`ClearAllInjections`) is still a big performance booster.

[Catalogue]: ../../../Documentation/CodeTutorials/Glossary.md#Catalogue

[CatalogueItem]: ../../../Documentation/CodeTutorials/Glossary.md#CatalogueItem
