# Localization

## Background
This document describes how localization is implemented in RDMP.  The design follows the [approach set out by Microsoft](https://docs.microsoft.com/en-us/dotnet/standard/globalization-localization/).

## Strings
All user visible strings in RDMP should be moved to GlobalStrings.resx

Language specific strings should appear in GlobalStrings.[language].resx e.g. GlobalStrings.zh-Hans.resx

## Naming Conventions
Since there are lot of strings in RDMP we will need to follow to ensure maintainability.

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

## Testing

You can test localization by adding the following to Program.cs

```csharp
Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-Hans");
Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hans");
```

## Outstanding Issues

- RDMP makes xmldoc comments visible through the UI (via `CommentStore`).  [Localizing this will be difficult](https://github.com/dotnet/roslyn/issues/3371).

