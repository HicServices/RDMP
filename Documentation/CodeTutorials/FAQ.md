# Frequently Asked Questions
## Table of contents
1. [How do I stop some nodes being reordered in RDMPCollectionUIs?](#reorder)
2. [How do I add new nodes to RDMPCollectionUIs?](#addNewNodes)
3. [How do platform databases / database objects work?](#databaseObjects)
4. [My metadata databases are being hammered by thousands of requests](#databaseDdos)
5. [How does RDMP handle untyped input (e.g. csv)?](#dataTypeComputer)
6. [Does RDMP Support Plugins?](#plugins)

<a name="reorder"></a>
### 1. How do I stop some nodes being reordered in RDMPCollectionUIs?
Sometimes you want to limit which nodes in an `RDMPCollectionUI` are reordered when the user clicks on the column header.  In the below picture we want to allow the user to sort data loads by name but we don't want to reorder the ProcessTask nodes or the steps in them since that would confuse the user as to the execution order.

![ReOrdering](Images/FAQ/ReOrdering.png) 

You can prevent all nodes of a given Type from being reordered (relative to their branch siblings) by inheriting `IOrderable` and returning an appropriate value:

```csharp
public class ExampleNode : IOrderable
{
	public int Order { get { return 2; } set {} }
}
```

If you are unsure what Type a given node is you can right click it and select 'What Is This?'.

<a name="addNewNodes"></a>
### 2. How do I add new nodes to RDMPCollectionUIs?
This requires a tutorial all of it's own 

https://github.com/HicServices/RDMP/blob/develop/Documentation/CodeTutorials/CreatingANewCollectionTreeNode.md


<a name="databaseObjects"></a>
### 3. How do platform databases / database objects work?

See `DataStructures.cd` (todo: How about a README.md - Ed)

<a name="databaseDdos"></a>
### 4. My metadata databases are being hammered by thousands of requests
The entire RDMP meta data model is stored in platform databases (Catalogue / Data Export etc).  Classes e.g. `Catalogue` are fetched either all at once or by `ID`.  The class Properties can be used to fetch other related objects e.g. `Catalogue.CatalogueItems`.  This usually does not result in a bottleneck but under some conditions deeply nested use of these properties can result in your platform database being hammered with requests.  You can determine whether this is the case by using the PerformanceCounter.  This tool will show every database request issued while it is running including the number of distinct Stack Frames responsible for the query being issued.  Hundreds or even thousands of requests isn't a problem but if you start getting into the tens of thousands for trivial operations you might want to refactor your code.

![PerformanceCounter](Images/FAQ/PerformanceCounter.png) 

Typically you can solve these problems by fetching all the required objects up front e.g.

```csharp
var catalogues = repository.GetAllObjects<Catalogue>();
var catalogueItems = repository.GetAllObjects<CatalogueItem>();
```

If you think the problem is more widespread then you can also use the `IInjectKnown<T>` system to perform `Lazy` loads which prevents repeated calls to the same property going back to the database every time.

https://github.com/HicServices/RDMP/blob/develop/Reusable/MapsDirectlyToDatabaseTable/Injection/README.md

<a name="dataTypeComputer"></a>
### 5. How does RDMP handle untyped input (e.g. csv)?

RDMP computes the data types required for untyped input as a `DataTypeRequest` using the `DataTypeComputer` class.  For full details see:

https://github.com/HicServices/RDMP/tree/develop/Reusable/ReusableLibraryCode/DatabaseHelpers/Discovery/TypeTranslation/README.md

<a name="plugins"></a>
### 6. Does RDMP Support Plugins?
Yes, RDMP supports both functional plugins (e.g. new anonymisation components, new load plugins etc) as well as UI plugins (e.g. new operations when you right click a `Catalogue`).

https://github.com/HicServices/RDMP/blob/develop/Documentation/CodeTutorials/PluginWriting.md
