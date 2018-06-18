# Frequently Asked Questions
**1. How is re-ordering prevented in TreeListViews/ObjectListViews**

   The class which represents the node in the list that you do not want to be re-ordered should inherit the IOrderable class and subsequently implement the inherited integer property called Order which returns the number representing the node's zero-based index:
   ```csharp
   public class ExampleNode : IOrderable
   {
		public int Order { get { return 2; } set {} }
   ```