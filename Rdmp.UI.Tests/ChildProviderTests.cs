// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rdmp.UI.Tests
{
    class ChildProviderTests : UITests
    {
        [Test]
        public void ChildProviderGiven_TableInfoWith_NullServer()
        {
            var ti = WhenIHaveA<TableInfo>();
            ti.Server = null;
            ti.SaveToDatabase();

            //creating a child provider when there are TableInfos with null servers should not crash the API!
            var provider = new CatalogueChildProvider(Repository.CatalogueRepository, null, new ThrowImmediatelyCheckNotifier(),null);
            var desc = provider.GetDescendancyListIfAnyFor(ti);
            Assert.IsNotNull(desc);

            //instead we should get a parent node with the name "Null Server"
            var parent = (TableInfoServerNode) desc.Parents[desc.Parents.Length - 1];
            Assert.AreEqual(TableInfoServerNode.NullServerNode, parent.ServerName);
        }

        [Test]
        public void TestUpTo()
        {
            string[] skip = {"AllAggregateContainers","_dataExportFilterManager","dataExportRepository","WriteLock","_oProjectNumberToCohortsDictionary","_errorsCheckNotifier", "ProgressStopwatch" };

            // We have 2 providers and want to suck all the data out of one into the other
            var cp1 = new DataExportChildProvider(RepositoryLocator,null,new ThrowImmediatelyCheckNotifier(),null);
            var cp2 = new DataExportChildProvider(RepositoryLocator,null,new ThrowImmediatelyCheckNotifier(),null);

            //to start with lets make sure all fields and properties are different on the two classes except where we expect them to be the same
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            foreach(var prop in typeof(DataExportChildProvider).GetProperties().Where(p => !skip.Contains(p.Name)))
            {
                var val1 = prop.GetValue(cp1);
                var val2 = prop.GetValue(cp2);

                // these are exempt, I guess 2 seperate empty arrays are now considered 'same'
                if(val1 is Array a1 && val2 is Array a2 && a1.Length == 0 && a2.Length == 0)
                    continue;

                Assert.AreNotSame(val1,val2,$"Prop {prop} was unexpectedly the same between child providers");
            }
                

            foreach(var field in typeof(DataExportChildProvider).GetFields(bindFlags).Where(p=>!skip.Contains(p.Name)))
            {
                var val1 = field.GetValue(cp1);
                var val2 = field.GetValue(cp2);

                // these are exempt, I guess 2 seperate empty arrays are now considered 'same'
                if(val1 is Array a1 && val2 is Array a2 && a1.Length == 0 && a2.Length == 0)
                    continue;

                Assert.AreNotSame(val1,val2,$"Field {field} was unexpectedly the same between child providers");
            }
                

            // Now call UpdateTo to make cp1 look like cp2
            cp1.UpdateTo(cp2);
            
            List<string> badProps = new List<string>();

            foreach(var prop in typeof(DataExportChildProvider).GetProperties().Where(p=>!skip.Contains(p.Name)))
                try
                {
                    Assert.AreSame(prop.GetValue(cp1),prop.GetValue(cp2),$"Prop {prop} was not the same between child providers - after UpdateTo");
                }
                catch (Exception)
                {
                    badProps.Add(prop.Name);
                }

            Assert.IsEmpty(badProps);
                        
            List<string> badFields = new List<string>();
            
            foreach(var field in typeof(DataExportChildProvider).GetFields(bindFlags).Where(p=>!skip.Contains(p.Name)))
                try
                {
                    Assert.AreSame(field.GetValue(cp1),field.GetValue(cp2),$"Field {field} was not the same between child providers - after UpdateTo");
                }
                catch(Exception)
                {
                    badFields.Add(field.Name);
                }
            
            Assert.IsEmpty(badFields);

        }
    }
}
