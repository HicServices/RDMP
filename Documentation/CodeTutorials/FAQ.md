# Frequently Asked Questions
# Table of contents
1. [How do I stop some nodes being reordered in RDMPCollectionUIs?](#reorder)
2. [How do I add new nodes to RDMPCollectionUIs?](#addNewNodes)

<a name="reorder"></a>
## 1. How do I stop some nodes being reordered in RDMPCollectionUIs?
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
## 2. How do I add new nodes to RDMPCollectionUIs?
This requires a tutorial all of it's own 

https://github.com/HicServices/RDMP/blob/develop/Documentation/CodeTutorials/CreatingANewCollectionTreeNode.md
