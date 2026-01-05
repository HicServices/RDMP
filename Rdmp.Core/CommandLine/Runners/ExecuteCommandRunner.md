# The 'cmd' Command

## Background

This document describes how RDMP interprets commands like:

```
> rdmp.exe cmd Set Catalogue:1 Name "My cool new name"
```

## Breakdown

The above command would be broken down as follows (see [ExecuteCommandOptions]):

| Operand | Role |
| ------------- | ------------- |
| `cmd` | Verb to pick an `IRunner` (in this case `ExecuteCommandRunner`).  Note that 'cmd' is the default verb and so can be skipped|
| `Set` | CommandName, determines which `BasicCommandExecution` derived Type to attempt to run.  If missing then an [interactive command loop] is triggered|
| `Catalogue:1 Name "My cool new name"`  | CommandArgs (3 in this case) for [constructing the command]|

[CommandArgs are optional](#running-without-arguments)
 
## Constructing the Command

First [CommandLineObjectPicker] processes the provided arguments to determine what they could be used as.

| Operand | Classification |
| ------------- | ------------- |
| `Catalogue:1` | `DatabaseEntity` (the `Catalogue` with `ID` 1) |
| `Name` | String|
| `My cool new name`  | String |
 
Note that some values can be classified as compatible with multiple different use cases (see [CommandLineObjectPickerArgumentValue])

For example `Catalogue` could either mean 'The `System.Type` called Catalogue' or 'a System.String value Catalogue' or 'all the `Catalogue` objects in the RDMP database' (e.g. `cmd list Catalogue`)

Once arguments are parsed for compatibility, [CommandInvoker] will pick a constructor and attempt to invoke it.  Consider the following constructor:

```csharp
[UseWithObjectConstructor]
public ExecuteCommandSet(IBasicActivateItems activator,IMapsDirectlyToDatabaseTable setOn,string property, string value):base(activator)
```

[CommandInvoker] can be called with or without a [CommandLineObjectPicker].  If no picker is passed then the [user will be prompted for each argument](#running-without-arguments) (via the `IBasicActivateItems`)

Each argument is either automatically populated (e.g. the `IBasicActivateItems`) or populated by consuming a [CommandLineObjectPickerArgumentValue] which must be of a compatible type.

| Parameter | Consumes Argument | Value Used|
| ------------- | ------------- |------------- |
| `IBasicActivateItems activator` | No | Automatically populated with the `ConsoleInputManager` |
| `IMapsDirectlyToDatabaseTable setOn` | Yes | `Catalogue:1`|
| `string property`  | Yes | `Name`|
| `string value`  | Yes | `My cool new name` |

This process must consume all provided parameters otherwise you will get an error like:

```
> rdmp.exe cmd Set Catalogue:1 Name NewName Lolazzz

Unrecognised extra parameter Lolazzz
```


## Interactive Command Loop

If you call `cmd` without passing a command you will be prompted to enter one.

```
> rdmp.exe cmd
Enter Command (or 'exit')
Command:
```

When entering a command you can either enter the whole command string:

```
> rdmp.exe cmd
Enter Command (or 'exit')
Command:
Set Catalogue:1 Name "My cool new name2"
```

Or you can enter only the command name in which case you will be prompted for required values

```
> rdmp.exe cmd
Enter Command (or 'exit')
Command:
Set
setOn
Enter value in one of the following formats:
Format: {Type}:{ID}[,{ID2},{ID3}...]
Format: {Type}:{NamePattern}[,{NamePattern2},{NamePattern3}...]
:
Catalogue:1
Value needed for parameter
property:Name
Value needed for parameter
value:Bob
2019-12-16 10:30:52.5972 DEBUG Save,Catalogue,1,System.String Name,My cool new name2,Bob .
Command Completed
```

## Running Without Arguments

If you only know the name of a command (but not its arguments) you can run `Describe`

```
> rdmp.exe cmd Describe Set
```

This will output useful information about the command e.g.:

```
COMMAND:Rdmp.Core.CommandExecution.AtomicCommands.ExecuteCommandSet
Changes a single property of an object and saves the new value to the database. New value must be valid for the object and respect any Type / Database constraints.
USAGE:
./rdmp.exe cmd Set <setOn> <property> <value>
PARAMETERS:
setOn   IMapsDirectlyToDatabaseTable    A single object on which you want to change a given property
property        String  Name of a property you want to change e.g. Description
value   String  New value to assign, this will be parsed into a valid Type if property is not a string
```

Alternatively you can run the command without specifying any arguments:

```
> rdmp.exe cmd Set
```

This will prompt you for each required argument in turn.

```
> rdmp.exe cmd Set

cmd set
...
setOn
Enter value in one of the following formats:
Format: {Type}:{ID}[,{ID2},{ID3}...]
Format: {Type}:{NamePattern}[,{NamePattern2},{NamePattern3}...]
:
```


[ExecuteCommandOptions]: ../Options/ExecuteCommandOptions.cs
[CommandLineObjectPicker]: ../Interactive/Picking/CommandLineObjectPicker.cs
[CommandLineObjectPickerArgumentValue]: ../Interactive/Picking/CommandLineObjectPickerArgumentValue.cs
[CommandInvoker]: ../../CommandExecution/CommandInvoker.cs
[interactive command loop]: #interactive-command-loop
[constructing the command]: #constructing-the-command

[Catalogue]: ../../../Documentation/CodeTutorials/Glossary.md#Catalogue
