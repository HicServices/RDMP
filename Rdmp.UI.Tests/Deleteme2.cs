using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.DataViewing;
using Rdmp.UI.MainFormUITabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.Tests
{
    public class CatalogueUITests2 : UITests
    {
        [Test, UITimeout(20000)]
        public void Test_CatalogueUI_NormalState()
        {
            //Get a new platform object
            var cata = WhenIHaveA<Catalogue>();
                        
            //Get an instance of the UI
            AndLaunch<CatalogueUI>(cata);
        }
    }
}
