# Coding RDMP



## Contents

1. [Background](#background)
2. [Libraries](#libraries)
   1. [Database Abstraction layer](#database-abstraction-layer)
   1. [Type Determination](#type-determination)
2. [Writing your first unit test](#writing-your-first-unit-test)
2. [Further Reading](#further-reading)

## Background
RDMP is a large codebase and can be quite intimidating at first glance.  The good news is functionality is well separated and there is plenty of documentation on features.

Make sure you can build and run the unit tests (see [README.md](./../../README.md#building))

## Libraries

Many of the complicated low level APIs have been refactored out of RDMP and moved to their own repositories.  These are consumed by RDMP during the build process via Nuget packages.

### Database Abstraction Layer

RDMP interacts with relational databases (Sql Server, Oracle, postgresql and MySql).  It runs SQL queries, creates tables and does general ETL.  This functionality has been abstracted out into the [FAnsiSql library](https://github.com/HicServices/FAnsiSql)

### Type Determination

Often we need to load untyped data (e.g. CSV) into a new table.  This requires picking [DBMS] appropriate types that will fit the content while still being useful for querying later on.  This functionality is handled by the [TypeGuesser](https://github.com/HicServices/TypeGuesser) library.  You can read more about how RDMP handles untyped data in [CSVHandling.md](./CSVHandling.md)

## Writing your first unit test

Often the easiest way to get into coding is to write simple unit tests.  Lets create one.

Start by [cloning the RDMP repository](https://docs.github.com/en/free-pro-team@latest/github/creating-cloning-and-archiving-repositories/cloning-a-repository).  Once it is on your computer create a new file in the Rdmp.Core.Tests project e.g. `./Rdmp.Core.Tests/MyTest.cs`:

```csharp
using NUnit.Framework;

namespace Rdmp.Core.Tests
{
    class MyTest
    {
        [Test]
        public void MyTestMethod()
        {
            TestContext.Out.WriteLine("Hello World");
            Assert.Fail("Goodbye cruel world");
        }
    }
}

```
_Hello world test_

Run your test from visual studio or or the command line with:

```csharp
dotnet test --filter MyTestMethod
```

Change the class to inherit from `UnitTests` and use the `WhenIHaveA<T>` method to get an RDMP object e.g. a [Catalogue]:

```csharp
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests
{
    class MyTest : UnitTests
    {
        [Test]
        public void MyTestMethod()
        {
            var catalogue = WhenIHaveA<Catalogue>();

            catalogue.Name = "Hi there";
            Assert.AreEqual("Hi there",catalogue.Name);
        }
    }
}
```
_Example unit test_

If you want to interact with a database e.g. test an extraction scenario you will need to [setup integration testing databases](./Tests.md).

## Further Reading

Each area of the RDMP codebase has its own documentation.  These include:

- [User Interface Overview](./UserInterfaceOverview.md)
  - [Creating New Tree Nodes](./CreatingANewCollectionTreeNode.md)
  - [Creating Right Click Context Menus](./CreatingANewRightClickMenu.md)
  - [Double Click / Drag and drop](./DoubleClickAndDragDrop.md)
- [Writing Plugins](./PluginWriting.md)
- [Cohort Creation](./../../Rdmp.Core/CohortCreation/CohortCreation.md)
- [Command Line Interface (CLI)](./../../Rdmp.Core/CommandLine/Runners/ExecuteCommandRunner.md)

RDMP has over 20 class diagrams which you can open if you have visual studio.  These files end in the extension `.cd`

If you have not already done so it is also a good idea to familiarise yourself with the [FAQ](./FAQ.md).

If there is an area of the codebase that is confusing or you think would benefit from more documentation, open an Issue and describe it!

[DBMS]: ./Glossary.md#DBMS
[Catalogue]: ./Glossary.md#Catalogue
