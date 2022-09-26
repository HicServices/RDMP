Here are things you should know about RDMP!

## Contents

1. [Other Docs](#other-docs)
1. [Commands](#Commands)
1. [Objects](#Objects)
1. [Windows Forms Designer](#windows-forms-designer)
1. [Icons and resx files](#icons-and-resx)
1. [Release Process](#release-process)


## Other Docs
All technical and repo specific documentation are stored in markdown (`.md` format).  Below is a list of docs in the repo.  There is also a Confluence website which stores [documentation on how to perform common user tasks](https://hic-docs.atlassian.net/wiki/spaces/RDMPDOCUME/overview?homepageId=196610)

**Code Tutorials**
- [Overview of RDMP Windows Client GUI application](./Documentation/CodeTutorials/UserInterfaceOverview.md)
- [Getting started Coding RDMP](./Documentation/CodeTutorials/Coding.md)
- [How to create 'non database' CohortBuilder plugins e.g. to REST API](./Documentation/CodeTutorials/CohortBuildingApiPlugins.md)
- [How to add new tree items to RDMP windows GUI client](./Documentation/CodeTutorials/CreatingANewCollectionTreeNode.md)
- [How to add new right click context menu items](./Documentation/CodeTutorials/CreatingANewRightClickMenu.md)
- [How to add new 'drag and drop' and double click handlers](./Documentation/CodeTutorials/DoubleClickAndDragDrop.md)
- [Adding Localization (i.e. foreign language support)](./Documentation/CodeTutorials/Localization.md)
- [Adding new RDMP plugins](./Documentation/CodeTutorials/PluginWriting.md)
- [How to write RDMP unit/integration tests](./Documentation/CodeTutorials/Tests.md)

**User Documentation**
- [Application Changelog](./CHANGELOG.md)
- [Main landing page README](./README.md)
- [Frequently Asked Questions](./Documentation/CodeTutorials/FAQ.md)
- [Glossary](./Documentation/CodeTutorials/Glossary.md)
- [RDMP DQE](./Documentation/CodeTutorials/Validation.md)
- [RDMP Command Line](./Documentation/CodeTutorials/RdmpCommandLine.md)
- [RDMP Command Line syntaxes](./Rdmp.Core/CommandLine/Runners/ExecuteCommandRunner.md)
- [RDMP upstream dependencies (libraries)](./Documentation/CodeTutorials/Packages.md)

** Performance **
- [Database Change Tracking (A Performance Enhancement)](./Documentation/CodeTutorials/ChangeTracking.md)
- [Reducing database calls with 'injection'](./Reusable/MapsDirectlyToDatabaseTable/Injection/README.md)

**Deep Dives**
- [How untyped CSV data is parsed by RDMP](./Documentation/CodeTutorials/CSVHandling.md)
- [How 'Bulk Insert' function works](./Documentation/CodeTutorials/DataTableUpload.md)
- [How xls / xlsx files are read by RDMP](./Documentation/CodeTutorials/ExcelHandling.md)
- [Multiple Linkage Columns (e.g. NHS Number or CHI)](./Documentation/CodeTutorials/MultipleExtractionIdentifiers.md)
- [Storing cohort lists](./Rdmp.Core/CohortCommitting/Readme.md)
- [Cohort Builder docs including info on list caching](./Rdmp.Core/CohortCreation/Readme.md)
- [Tree layout documentation](./Rdmp.Core/Providers/Readme.md)

## Commands
The [Design Pattern](https://en.wikipedia.org/wiki/Software_design_pattern) 'Command' is implemented in RDMP.  Most functionality in RDMP is undertaken by a command and new features should be implemented as new commands if possible. 
All commands implement `IAtomicCommand` - it's a good place to start when diagnosing issues.

- If you want to see which command is running when an issue manifests in the windows UI you can use the menu item 'Diagnostics->Last Command Monitor'
- Commands have access to `IBasicActivateItems` which contains modal operations for illiciting user feedback.  Bear in mind that some implementations do not support interactive content so always check `IBasicActivateItems.IsInteractive`
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

## Icons and ResX

In 2001 Microsoft announced the [deprecation of System.Drawing.Common](https://github.com/dotnet/designs/pull/234).  The suggested migration path was to move to alternative libraries e.g. [ImageSharp](https://github.com/SixLabors/ImageSharp).

RDMP followed this approach.  The update was merged in [#1355](https://github.com/HicServices/RDMP/pull/1355).  This means that all resx files must store `Byte[]` and code must be manually updated if images are added using the designer.

The approach for adding new icons is:

- Open resx file in designer (e.g. CatalogueIcons.resx)
- Drop new image into designer and save

This creates a new entry in .resx file for example:

```xml
<data name="YourImage" type="System.Resources.ResXFileRef, System.Windows.Forms">
    <value>..\YourImage.png;System.Drawing.Bitmap, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</value>
  </data>
```

This must be updated to be a raw `Byte[]`.  Change the `value` tag by replacing everything after the file path (first semicolon) with `;System.Byte[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>`

For example 
```
<data name="YourImage" type="System.Resources.ResXFileRef, System.Windows.Forms">
    <value>..\YourImage.png;System.Byte[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </data>
```

Next update the `.Designer.cs` file (e.g. `CatalogueIcons.Designer.cs`).  Add a new entry for the new resx file:

```csharp
/// <summary>
///   Looks up a localized resource of type Image.
/// </summary>
public static Byte[] YourImage
{
    get
    {
        object obj = ResourceManager.GetObject("YourImage", resourceCulture);
        return ((Byte[])(obj));
    }
}
```

This `Byte[]` can then be turned into either a Windows Forms compatible Bitmap (windows only):

```csharp
 CatalogueIcons.YourImage.ImageToBitmap();
```

Or a cross platform ImageSharp `Image` with:

```csharp
Image.Load<Rgba32>(CatalogueIcons.YourImage);
```

## Release Process
RDMP releases are performed by GitHub Actions CI server.  The logic for this is in [build.yml](./.github/workflows/build.yml).

To perform a release merge all branches into `develop` then perform the following:

- Update [Changelog](./CHANGELOG.md)
  - Add a header with the version number and date e.g. `## [7.0.21] - 2022-09-26`
  - Add a diff link at the bottom e.g. `[7.0.21]: https://github.com/HicServices/RDMP/compare/v7.0.20...v7.0.21`
- Update [SharedAssemblyInfo](./SharedAssemblyInfo.cs)
  - Update all 3 versions to your new number e.g. `7.0.21`
- Commit and push

Next tag the release with `git` as the new version number.  Do not forget the `v` prefix e.g.

```
git tag v7.0.21
git push --tags
```

This should push a new release to both `NuGet` and `GitHub Releases`.  If you have a problem with the build CI, you can [delete the local AND remote tag](https://devconnected.com/how-to-delete-local-and-remote-tags-on-git/) then fix the problem then re-tag.

Once the release is built and you have tested the binary in [GitHub Releases](https://github.com/HicServices/RDMP/releases) you can make it live by updating [rdmp-client.xml](./rdmp-client.xml)

- Update the `version` tag
- Update the `url` tag to the new version
- Update the `changelog` tag to have the new version anchor hyperlink

Test that your RDMP client can update with `Help->Check for updates` (note that if your xml file change is on `develop` you will need to specify this as the update path)

Finally merge `develop` into `main` and push.  This will ensure that the `main` branch always has the source of the last RDMP version release.

[YamlRepository]: /Rdmp.CoreRepositories/YamlRepository.cs
[CatalogueRepository]: ./Rdmp.CoreRepositories/CatalogueRepository.cs
[DataExportRepository]: ./Rdmp.Core/Repositories/DataExportRepository.cs
[DQERepository]: ./Rdmp.Core/Repositories/DQERepository.cs
[Project]: ./Documentation/CodeTutorials/Glossary.md#Project