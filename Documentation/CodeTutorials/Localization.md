# Localization

## Background
This document describes how localization is implemented in RDMP.  The design follows the [approach set out by Microsoft](https://docs.microsoft.com/en-us/dotnet/standard/globalization-localization/).

## Strings
All user visible strings in RDMP should be moved to GlobalStrings.resx

Language specific strings should appear in GlobalStrings.[language].resx e.g. GlobalStrings.zh-Hans.resx

## Naming Conventions
Since there are lot of strings in RDMP we will need to follow to ensure maintainability.

## Shared roots

Where multiple strings are related they must have the same root e.g.:

- CreateArchiveTableCaption
- CreateArchiveTableYesNo
- CreateArchiveTableSuccess


## Parameterized strings

Where a string includes reference to a dynamic value e.g.

```
"Table Bob did not exist"
```

It should be stored in the resource file(s) as:

```
"Table {0} did not exist"
```

_This allows it to be used with String.Format_

### Menu Items

All menu items in RDMP should be modelled as a `IAtomicCommand`.  This class has the following methods which should be implemented:

```csharp
public class ExecuteCommandCreateNewCatalogueByImportingFile:BasicUICommandExecution, IAtomicCommandWithTarget
{
    ...

    public override string GetCommandHelp()
    {
        return GlobalStrings.CreateNewCatalogueByImportingFileHelp;
    }

    public override string GetCommandName()
    {
        return GlobalStrings.CreateNewCatalogueByImportingFile;
    }
}
```

__The resource property name(s) should match the command name__.

### Enums

There are cases in RDMP where the string values of Enums are used.  Use the extension method `S()` to display a culture specific (if exists) string.

For example

```
MessageBox.Show("Status was:" + TriggerStatus.Disabled.S());
```

## Testing

You can test localization by adding the following to Program.cs

```csharp
Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-Hans");
Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hans");
```

## Unit Testing

Visual Studio plugin ReSharper shows missing translations and there are many good tools for interacting with resx files (even those aimed at translaters rather than programmers) so the amount of unit testing we need to do should be quite limited.

- We should write a test that confirms that all commands with tokens ({0},{1} etc) have the same number of distinct tokens in all languages (implementing overrides)
- Maybe we can make `EvaluateNamespacesAndSolutionFoldersTests` detect incorrect usages of `Show` , `YesNo` where params don't match the number of {x} tokens too

## Outstanding Issues

- RDMP makes xmldoc comments visible through the UI (via `CommentStore`).  [Localizing this will be difficult](https://github.com/dotnet/roslyn/issues/3371).

