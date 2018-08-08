# Table of contents
1. [Background](#background)
2. [Activation](#activation)
3. [Tab Documents](#tab-documents)
4. [Drop](#drop)
4. [Drag](#drag)

<a name="background"></a>
# Background
Drag and drop and double clicking (called activation) is a core part of the RDMP API and is handled through the class `RDMPCommandExecutionProposal<T>` on a `Type` basis.  Each object that supports either activation or drop must have an associated instance of `RDMPCommandExecutionProposal<T>` called `ProposeExecutionWhenTargetIs<SomeClass>`.  

This derrived class will decide what tab/window/custom action to show when `Activate` happens either as part of double click or as part of `ExecuteCommandActivate` (e.g. from a right click menu) or a call to `BasicUICommandExecution.Activate` and decide what `ICommandExecution` is executed when a given object/collection is dropped on it.

![ExampleMenu](Images/DoubleClickAndDragDrop/DropExample.png) 

This pattern allows all tree views system wide to have consistent behaviour for a given object type (via `RDMPCollectionCommonFunctionality`).  

For the developer it allows easy troubleshooting, if there is a problem with a tree object right click it and select 'What is this?' then search for the class `ProposeExecutionWhenTargetIs<ClassName>`.  Since the return type of the drop method `ProposeExecution` is `ICommandExecution` you simply have to decide what the appropriate command is for the drop operation.

See [ProposeExecutionWhenTargetIsExtractionConfiguration](https://github.com/HicServices/RDMP/blob/94591a458e3fb3233039a22b8f2e16e8a31f83bf/DataExportManager/DataExportManager/CommandExecution/Proposals/ProposeExecutionWhenTargetIsExtractionConfiguration.cs) for an example of how consise the code is.

# Activation
To add support for activating (double clicking) an object we must first know it's `Type`.  Right click it in the tree view and select 'What is this?' to determine it's `Type`.

![ExampleMenu](Images/DoubleClickAndDragDrop/WhatIsThis.png)

Create a new class called `ProposeExecutionWhenCommandIs<TypeName>` in namespace `CatalogueManager.CommandExecution.Proposals` and inherit from `RDMPCommandExecutionProposal<T>`.  Return true for `CanActivate` and put a test message in `Activate`.

```csharp
namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenCommandIsPipeline:RDMPCommandExecutionProposal<Pipeline>
    {
        public ProposeExecutionWhenCommandIsPipeline(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(Pipeline target)
        {
            return true;
        }

        public override void Activate(Pipeline target)
        {
            MessageBox.Show("Double clicked");
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, Pipeline target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}

```

Check this works in the application by double clicking a tree node of the appropriate `Type` or right clicking and selecting `Edit`.

![ExampleMenu](Images/DoubleClickAndDragDrop/TestMessage.png)

# Tab Documents
Most tabs in RDMP are designed for editing/executing a single `Type` of object held in one of the RDMP platform database tables (e.g. Catalogue, CatalogueItem, Project etc).  By convention these controls should be named `<MyClass>UI` unless there is a good reason not to.  These user interfaces all inherit from abstract base class `RDMPSingleDatabaseObjectControl<T>`.  There can be multiple tabs for a given `Type` e.g. `CacheProgress` has both `CacheProgressUI` (for changing cache dates etc) and `ExecuteCacheProgressUI` (for executing the cache).

To show a new tab control for editing your object you should create a normal WinForms control as you normally would but inherit from `RDMPSingleDatabaseObjectControl<T>` instead of `UserControl`.

To trigger the user interface on `Activate` of an `RDMPCommandExecutionProposal<T>` call `ItemActivator.Activate` 

```csharp
public override void Activate(AggregateConfiguration target)
{
    ItemActivator.Activate<AggregateEditor, AggregateConfiguration>(target);
}
```

If you find you cannot use Visual Studio Designer to edit your control because of the abstract/generic base class `RDMPSingleDatabaseObjectControl` [you can create an AbstractControlDescriptionProvider with an intermediate class](https://stackoverflow.com/questions/1620847/how-can-i-get-visual-studio-2008-windows-forms-designer-to-render-a-form-that-im).

```csharp
[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ANOTableUI_Design, UserControl>))]
public abstract class ANOTableUI_Design : RDMPSingleDatabaseObjectControl<ANOTable>
{
}
```

# Drop
To add support for item dropping you should add an implementation to the body of the `ProposeExecution` method.  The method provides the following parameters.

| Parameter | Purpose |
| ------------- | ------------- |
| ICommand cmd| Self contained class describing both the object being dragged and salient facts about it e.g. if  it is a `CatalogueCommand` then it will know whether the dragged `Catalogue` has at least one patient identifier column.|
| T target | The object the cursor is currently hovering over |
| InsertOption insertOption | Whether the cursor is above or below or ontop of your object (if the collection the object is in supports it) |

The reason we have an `ICommand` is so we can front load discovery and encapsulate facts into a single class which can then be waved around the place to look for valid combinations.  If an object doesn't have an associated `ICommand` then it won't be draggable in the first place.

To add support for dropping an object with an existing `ICommand` you should cast `cmd` and return an appropriate `ICommandExecution`.

```csharp
public override ICommandExecution ProposeExecution(ICommand cmd, Pipeline target, InsertOption insertOption = InsertOption.Default)
{
	var sourceCatalogueCommand = cmd as CatalogueCommand;
	
	if(sourceCatalogueCommand != null)
		return new ExecuteCommandDelete(ItemActivator,sourceCatalogueCommand.Catalogue);

	return null;
}
```

While not terribly useful, you can now drop a `Catalogue` on `Pipeline` to delete the `Catalogue`

![ExampleMenu](Images/DoubleClickAndDragDrop/DropDelete.png)

You can create your own `ICommandExecution` implementations by [following this tutorial](./CreatingANewRightClickMenu.md#creating-a-new-command)

# Drag

TODO