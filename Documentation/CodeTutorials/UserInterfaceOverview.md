# User Interface Overview

## Background

RDMP is a large application with many complicated user interfaces.  In order to manage these a strict architecture has been built.  This architecture is bult around a single base class `IRDMPControl` for all top level controls.  This interface provides access to the UI API `IActivateItems`.  Unit testing of UIs is supported via `TestActivateItems`.
![user interface class diagram](Images/UserInterfaceOverview/ClassDiagram.png) 

## Collections

<img align="left" width="240" height="450" src="Images/UserInterfaceOverview/ExampleCollection.png">

`RDMPCollectionUI` is responsible for hosting tree views depicting all objects associated with a given area of the program (Extraction,Cohort Building, Loading etc).  There can only be one instance of any given `RDMPCollectionUI` at once (like the Solution Explorer in Visual Studio).

All the logic for how these controls operate is defined in `RDMPCollectionCommonFunctionality`.  The hierarchy of objects is determined up front by `ICoreChildProvider`.  The hierarchy is updated whenever any [Publish](#publishing) occurs.

<div style="clear:both;"></div>

## Single Object Control Tabs



Where possible RDMP associates any top level tab (that isn't a [collection](#collections)) with a single object.  This is done by subclassing `RDMPSingleDatabaseObjectControl<T>` with the `DatabaseEntity` class being managed.  The flow of control for these tabs is:

1. Blank constructor invoked
1. Control shown on Form
1. SetDatabaseObject entered in derrived class
   1. `base.SetDatabaseObject` invoked by derrived class
   1. `SetBindings` called (virtual)
1. SetDatabaseObject exited

Whenever a Publish event occurs `SetDatabaseObject` is invoked again with the latest version of the hosted object.  If the hosted object is deleted then the tab is automatically closed.

You can instigate this process by calling `IActivateItems.Activate<T, T2>(T2 databaseObject)`  Where `T` is the object that should be shown in the UI control of Type `T2`

Only one instance of a given UI Type can be open at once per databaseObject (i.e. you cannot have two `CatalogeUI` tabs showing the same `Catalogue`).  If you try to do this the `IActivateItems` will just bring the existing instance to the front.

Tabs are automatically persisted when exiting the program by recording the Type of the UI and the ID of the object being hosted.

### Task Bar

![example single control tab](Images/UserInterfaceOverview/ExampleSingleObjectControlTab.png) 
*Example Single Object Control Tab*

You can add buttons and menu items to the top task bar in `SetDatabaseObject` using:

```csharp

//Adds command to the dropdown menu
CommonFunctionality.AddToMenu(new ExecuteCommandViewFilterMatchData(Activator, databaseObject, ViewType.TOP_100));
CommonFunctionality.AddToMenu(new ExecuteCommandViewFilterMatchData(Activator, databaseObject, ViewType.Aggregate));

//Adds the command to the top bar
CommonFunctionality.Add(new ExecuteCommandViewFilterMatchData(Activator, databaseObject, ViewType.TOP_100));
CommonFunctionality.Add(new ExecuteCommandViewFilterMatchData(Activator, databaseObject, ViewType.Aggregate));

//Adds checks for the current object
CommonFunctionality.AddChecks(databaseObject);

```

If your control supports modifying the Properties of the hosted object you should declare that you implement `ISaveableUI` (methods are already declared by base class).  This will result in the save/undo control being added to the task bar.

Since `SetDatabaseObject` is called when relevant changes are [Published](#publishing), the task bar is also repopulated.  This is intended since it gives an opportunity for [commands](#commands) to be recreated reflecting the new system state (e.g. `IsImpossible` might change).

## Other Tabs
Sometimes it is not possible to tie a UI control to a single object (e.g. `ViewSQLAndResultsWithDataGridUI`).  This may be because the UI reflects a collection of objects combined or a choice the user made when launching the control.  In this case the UI should implement `IObjectCollectionControl` and declare an `IPersistableObjectCollection` which contains all the objects and settings needed to persist the UI.

It is possible to show adhoc untracked UI controls as tabs via `IActivateItems.ShowWindow` but this is discouraged.

## Publishing
The `RefreshBus` is responsible for updating all UI components with the latest state of objects.  This is instigated whenever `RefreshBus.Publish` is called by a UI.  The sequence of events is:

1. `ICoreChildProvider` updated (fetches all new objects from the repository)
2. BeforePublish event called
3. All `IRefreshBusSubscriber` are notified

Typical behaviours exhibited by subscribers include closing (if the object has been deleted) or updating the displayed contents based on the new state of the object.

Subscribers should never attempt a new Publish while responding to a Publish callback (this will trigger a `SubscriptionException`).

Since the Equality comparer of `DatabaseEntity` considers only the `ID` and `Repository` newly published objects do not break Dictionaries / HashSets etc.  This is very important since [most UI tabs are tied to a specific single object](#single-object-control-tabs).


## Double Clicking

## Drag and Drop

## Menus

# Commands
Where possible RDMP likes to encapsulate all atomic operations that the user can perform in an `IAtomicCommand` implementation e.g. `ExecuteCommandCreateNewFilter`.  You should inherit from `BasicUICommandExecution` and override the required methods.

`IAtomicCommand` objects can be displayed in several ways and are the preferred way of offering options to the user.



