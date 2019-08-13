using BrightIdeasSoftware;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.Collections;
using Rdmp.UI.MainFormUITabs;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.Tests
{
    class ArbitraryFolderNodeTests:UITests
    {

        [Test,UITimeout(5000)]
        public void Test_ArbitraryFolderNode_CommandGetter_Throwing()
        {
            SetupMEF();

            var tlv = new TreeListView();
            var common = new RDMPCollectionCommonFunctionality();
            common.SetUp(RDMPCollection.None,tlv,ItemActivator,null,null);

            var node = new ArbitraryFolderNode("my node",0);

            var menu1 = common.GetMenuIfExists(node);
            Assert.IsNotNull(menu1);
            int count1 = menu1.Items.Count;
            //some you get for free e.g. Expand/Collapse
            Assert.GreaterOrEqual(count1,2);

            //set the menu to have one command in it
            node.CommandGetter = () =>  new IAtomicCommand[] { new ImpossibleCommand("Do Nothing")};

            var menu2 = common.GetMenuIfExists(node);

            int count2 = menu2.Items.Count;
            Assert.AreEqual(count1+1,count2);
            
            //what happens if the delegate crashes?
            node.CommandGetter = () => throw new NotSupportedException("It went wrong!");

            Assert.DoesNotThrow(()=>common.GetMenuIfExists(node));

            AssertErrorWasShown(ExpectedErrorType.GlobalErrorCheckNotifier, "Failed to build menu for my node of Type Rdmp.Core.Providers.Nodes.ArbitraryFolderNode");
        }

    }
}
