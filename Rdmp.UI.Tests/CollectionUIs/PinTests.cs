using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.Tests.CollectionUIs
{
    class PinTests : UITests
    {

        [Test,UITimeout(50000)]
        public void Test_DeletePinnedObject_PinClears()
        {
            var ti = WhenIHaveA<TableInfo>();

            var collection = AndLaunch<TableInfoCollectionUI>();
            
            //nothing selected to start with
            Assert.IsNull(collection.CommonTreeFunctionality.Tree.SelectedObject);
            
            //emphasise and pin it (like Ctrl+F to object)
            ItemActivator.RequestItemEmphasis(this,new ItemActivation.Emphasis.EmphasiseRequest(ti){Pin = true });

            //the TableInfo should now be selected
            Assert.AreEqual(ti,collection.CommonTreeFunctionality.Tree.SelectedObject);

            //and pinned
            Assert.AreEqual(ti,collection.CommonTreeFunctionality.CurrentlyPinned);

            //delete the object
            ItemActivator.DeleteWithConfirmation(this,ti);

            //selection should now be cleared
            Assert.IsNull(collection.CommonTreeFunctionality.Tree.SelectedObject);

            //as should the pin
            Assert.IsNull(collection.CommonTreeFunctionality.CurrentlyPinned);

        }
    }
}
