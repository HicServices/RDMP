Here are things you should know about RDMP!

- Most RDMP functionality is driven by the Command pattern.  All commands
  implement `IAtomicCommand` - it's a good place to start when diagnosing issues
- All the docs are in .md files, start by looking in Documentation\CodeTutorials

Windows Forms Designer
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
