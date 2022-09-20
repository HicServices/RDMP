Here are things you should know about RDMP!

## Contents

1. [Commands](#Commands)
1. [Objects](#Objects)
1. [Windows Forms Designer](#windows-forms-designer)

All the docs are in .md files, start by looking in Documentation\CodeTutorials

## Commands
The [Design Pattern](https://en.wikipedia.org/wiki/Software_design_pattern) 'Command' is implemented in RDMP.  Most functionality in RDMP is undertaken by a command and new features should be implemented as new commands if possible. 
All commands implement `IAtomicCommand` - it's a good place to start when diagnosing issues.

- If you want to see which command is running when an issue manifests in the windows UI you can use the menu item 'Diagnostics->Last Command Monitor'
- Commands have access to `IBasicActivateItems` which contains modal operations for illiciting user feedback.  Bear in mind that some implementations do not support interactive content so always check `IBasicItems.IsInteractive`
- UI implementations are
  - `IActivateItems`: windows gui client
  - `ConsoleInputManager`: [CLI client](./Documentation/CodeTutorials/RdmpCommandLine.md).  May be [running in a script](./Documentation/CodeTutorials/RdmpCommandLine.md#scripting)
  - `ConsoleGuiActivator`: TUI user interface (cross platform - great for SSH/headless servers)
  - `TestActivateItems`: Implementation that can be used in UI tests

## Objects 

RDMP objects inherit from `DatabaseEntity`.  There are multiple types of `IRepository` e.g [YamlRepository], [CatalogueRepository].  
When there is a database backed object store (e.g. [CatalogueRepository], [DataExportRepository], [DQERepository]) then it is important
to load/save/create with the correct one.  For example you cannot save an `Project` to [CatalogueRepository], only [DataExportRepository].
If you want to see what repository type it is meant to be then look at the constructor e.g. `public Project(IDataExportRepository repository, string name)`

- In the GUI client if you are ever wondering what an object type is you can right click it and choose 'What is this?'
- All objects have a unique ID.  The uniqueness is restricted to the `IRepository` it is in.


## Windows Forms Designer
------------------------------------
- Using the Windows Forms Designer requires renaming SelectedDialog`1.resx to
  SelectedDialog.resx first.  Otherwise you get this bug:
  https://github.com/HicServices/RDMP/issues/1360
- If creating a new Control or Form you should consider inheriting from
  `RDMPSingleDatabaseObjectControl<T>` or `RDMPUserControl`.  If you do
  this make sure to declare an appropriate `TypeDescriptionProvider`
  (see below).  Otherwise it will not open in Designer.

```csharp
[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoggingTab_Design, UserControl>))]
public abstract class LoggingTab_Design : RDMPSingleDatabaseObjectControl<YourSingleObjType>
{

}
```


[YamlRepository]: /Rdmp.CoreRepositories/YamlRepository.cs
[CatalogueRepository]: ./Rdmp.CoreRepositories/CatalogueRepository.cs
[DataExportRepository]: ./Rdmp.Core/Repositories/DataExportRepository.cs
[DQERepository]: ./Rdmp.Core/Repositories/DQERepository.cs