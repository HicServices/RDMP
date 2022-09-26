# Creating A Context Menu

## Contents
1. [Background](#background)
2. [Cross Platform Menus (Recommended)](#cross-platform)
2. [Windows GUI Only Menus](#windows-only)
2. [Masquerading](#masquerading)
5. [Example](#example)
6. [Commands](#commands)
7. [Creating a new Command](#creating-a-new-command)

## Background
RDMP uses tree collections to represent all objects the user is required to interact with (filters, datasets, projects, extraction configurations etc).  Every item can be right clicked resulting in a context menu appropriate to the object.  At a minimum it should have Expand/Collapse and 'What is this?'.

![ExampleMenu](Images/CreatingANewRightClickMenu/ExampleMenu.png) 

This menu is built by `RDMPCollectionCommonFunctionality` using reflection.

<a name="cross-platform"/>
## Cross Platform Menus (Recommended)
The recommended way of adding new menu items is to create a new class implementing `BasicCommandExecution`.  See [ExecuteCommandList](../../Rdmp.Core/CommandExecution/AtomicCommands/ExecuteCommandList.cs)for a simple example of how to do this.  All commands should be declared in `Rdmp.Core.CommandExecution.AtomicCommands`

After creating the command update [AtomicCommandFactory] to `yield` it for the appropriate object type.

<a name="windows-only"/>
## Windows GUI Only Menus

If you require windows specific features that are not available through the
`IBasicActivateItems` abstraction then you can create UI only commands.  These
should be declared in `Rdmp.UI.CommandExecution.AtomicCommands`.  Since [AtomicCommandFactory]
is declared in `Rdmp.Core` you will not be able to serve the command in the same way.

Instead you should declare or locate the menu class for your object (whose menu you want to expose the new command
on).  For example if your new command is intended for [Catalogue] then add it to `CatalogueMenu`.

If you have to define your own menu from scratch (i.e. there is no existing menu called MyObjectMenu) then you must adhere
to the following:

1. If an object called `MyObject` has a menu it must be called `MyObjectMenu` and must inherit from `RDMPContextMenuStrip`
2. It must have a constructor signature `public MyObjectMenu(RDMPContextMenuStripArgs args, MyObject instance)`
3. It must be in namespace `Rdmp.UI.Menus`

If no corresponding menu exists for the object then the default menu will be shown.

## Masquerading
Sometimes we want 2 objects to have the same menu without having to create two menu classes.  This is valid if they represent the same object e.g. [ColumnInfo] and  `LinkedColumnInfoNode` (this represents a specific [ColumnInfo] to [CatalogueItem] binding).  The following options are available to make this happen.

1. Making the wrapper class `IMasqueradeAs` will allow you to specify at the class level that it behaves like the wrapped object with the following exceptions
1. If you implement `IDeletable` then your method will be called (not the object you are masquerading as)
2. If a menu directly compatible with your wrapper class does exists then it will be used (i.e. the masqueraded object menu is only used if there isn't one for the wrapper)
	
## Example
Right click the "Data Repository Servers" folder and click "What is this?" (See [picture in Background](#background)).  A MessageBox should pop up saying `AllServersNode`

Create a new class in `Rdmp.UI.Menus` called `AllServersNodeMenu` 

```csharp
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;

namespace Rdmp.UI.Menus
{
    class AllServersNodeMenu : RDMPContextMenuStrip
    {
        public AllServersNodeMenu(RDMPContextMenuStripArgs args, AllServersNode o) : base(args, o)
        {
            Items.Add(new ToolStripMenuItem("Do Nothing"));
        }
    }
}
```

Run RDMP and right click the `AllServersNode` again, you should see your new menu item.

![ExampleMenu](Images/CreatingANewRightClickMenu/DoNothingMenuItem.png)

## Commands
The preferred way of adding menu items is to use abstract base class `RDMPContextMenuStrip` method `Add(IAtomicCommand cmd)`.  For example 

<!--- f243e95a6dc94b3486f44b8f0bb0ed7d --->
```csharp
class AllServersNodeMenu : RDMPContextMenuStrip
{
    public AllServersNodeMenu(RDMPContextMenuStripArgs args, AllServersNode o) : base(args, o)
    {
        Add(new ExecuteCommandCreateNewEmptyCatalogue(args.ItemActivator));
    }
}
```
This delegates all display/execution logic to `IAtomicCommand` and lets reuse the command elsewhere (e.g. in other menus).  

![ExampleMenu](Images/CreatingANewRightClickMenu/AddCommand.png)

## Creating a new Command

All commands must be declared in an appropriate namespace e.g. `CommandExecution` (the unit test `UserInterfaceStandardisationChecker` will tell you if you put it in the wrong place).

If you want to create a new command then you should 
1. Create a new class called `ExecuteCommand<Something>` inherit from `BasicUICommandExecution` and implement `IAtomicCommand`
2. Declare a constructor taking whatever arguments are required to do the command and an `IActivateItems` (for calling the base constructor)
3. Override `GetImage` returning null for no image or using the `IIconProvider` to provide a suitable `RDMPConcept` / `OverlayKind`
4. Override `Execute` to perform the command logic (make sure to still call `base.Execute` so that `IsImpossible` is respected)

In your constructor you can decide that the arguments are not compatible with the command.  In this case you should call `SetImpossible` to indicate that the command cannot be run with its current arguments / environment state.  This will result in the command being greyed out in menus (and the user will be able to hover over to see why).  This is the preferred approach rather than not adding the command in the first place.

![ExampleMenu](Images/CreatingANewRightClickMenu/IsImpossible.png)

Useful methods you can invoke in `Execute` include

1. `Publish` which will entire user interface to respect a new object (or a change to an existing one).  Do not call this in a loop!
2. `Emphasise` Navigates the user interface to the collection containing the object and selects it (brings the object to the attention of the user)
4. `Activate` performs the double click action for the object usually resulting in a new tab opening for editing/executing the object
3. `SelectOne<T>` gets the user to pick a `DatabaseEntity` from a selection (or all the ones of a given type in the database).

[ColumnInfo]: ./Glossary.md#ColumnInfo
[CatalogueItem]: ./Glossary.md#CatalogueItem
[Catalogue]: ./Glossary.md#Catalogue
[AtomicCommandFactory]: ../../Rdmp.Core/CommandExecution/AtomicCommandFactory.cs