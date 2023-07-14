// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.ArgumentTests;

public class ProcessTaskTests : DatabaseTests
{

    [Test]
    public void CloneProcessTask_ToSameLoadMetadataWithoutArguments()
    {
        var test = new LoadMetadata(CatalogueRepository);
        var processTask1 = new ProcessTask(CatalogueRepository, test, LoadStage.AdjustRaw)
        {
            Name = "Franky",
            Order = 999
        };

        try
        {
            processTask1.SaveToDatabase();

            var clone = processTask1.CloneToNewLoadMetadataStage(test,LoadStage.GetFiles);
            Assert.AreNotSame(clone.ID, processTask1.ID);
            Assert.IsFalse(clone.ID == processTask1.ID);

            //get fresh copy out of database to ensure it is still there
            var orig = CatalogueRepository.GetObjectByID<ProcessTask>(processTask1.ID);
            clone = CatalogueRepository.GetObjectByID<ProcessTask>(clone.ID);

            Assert.IsFalse(orig.ID == clone.ID);
            Assert.AreEqual(LoadStage.AdjustRaw,orig.LoadStage);
            Assert.AreEqual(LoadStage.GetFiles, clone.LoadStage);

            Assert.AreEqual(orig.Order,clone.Order);
            Assert.AreEqual(orig.Path, clone.Path);
            Assert.AreEqual(orig.ProcessTaskType, clone.ProcessTaskType);
            Assert.AreEqual(orig.LoadMetadata_ID, clone.LoadMetadata_ID);

            clone.DeleteInDatabase();
        }
        finally 
        {
            processTask1.DeleteInDatabase();
            test.DeleteInDatabase();
        }
    }

    [Test]
    public void CloneProcessTask_ToNewLoadMetadataWithArguments()
    {
        //setup parents
        var parent1 = new LoadMetadata(CatalogueRepository);
        var parent2 = new LoadMetadata(CatalogueRepository);
            
        //make sure we didn't magically create the same ID somehow 
        Assert.AreNotEqual(parent1.ID,parent2.ID);

        //setup things to clone in parent1
        var processTask1 = new ProcessTask(CatalogueRepository, parent1, LoadStage.AdjustRaw);
        var arg = new ProcessTaskArgument(CatalogueRepository, processTask1)
        {
            Name = "TestArg"
        };
        arg.SetType(typeof (string));
        arg.SetValue("TestValue");
        arg.SaveToDatabase();

        processTask1.Name = "Franky";
        processTask1.Order = 999;
        processTask1.SaveToDatabase();
            
        try
        {
            //clone to parent 2
            var clone = processTask1.CloneToNewLoadMetadataStage(parent2, LoadStage.GetFiles);
            Assert.AreNotSame(clone.ID, processTask1.ID);
            Assert.IsFalse(clone.ID == processTask1.ID);

            //////////////////////////////////////////////////////////////////CHECK CLONAGE OF PROCESS TASK ////////////////////////////////////////////////////////////
            //get fresh copy out of database to ensure it is still there
            var orig = CatalogueRepository.GetObjectByID<ProcessTask>(processTask1.ID);
            clone = CatalogueRepository.GetObjectByID<ProcessTask>(clone.ID);
                
            //ids must have changed
            Assert.IsFalse(orig.ID == clone.ID);

            //load stages must be correct per what we requested
            Assert.AreEqual(LoadStage.AdjustRaw, orig.LoadStage);
            Assert.AreEqual(LoadStage.GetFiles, clone.LoadStage);
                
            //all regular values must have been cloned successfully
            Assert.AreEqual(orig.Order, clone.Order);
            Assert.AreEqual(orig.Path, clone.Path);
            Assert.AreEqual(orig.ProcessTaskType, clone.ProcessTaskType);

            Assert.AreEqual(parent1.ID, orig.LoadMetadata_ID);
            Assert.AreEqual(parent2.ID, clone.LoadMetadata_ID);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////CHECK CLONAGE OF ARGUMENTS ////////////////////////////////////////////////////////////

            var clonearg = clone.ProcessTaskArguments.SingleOrDefault(); 
            Assert.NotNull(clonearg);

            Assert.AreNotEqual(clonearg.ID,arg.ID);
            Assert.AreEqual(clonearg.GetType(),arg.GetType());
            Assert.AreEqual(clonearg.Name,arg.Name);
            Assert.AreEqual(clonearg.Value,arg.Value);
                
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            clone.DeleteInDatabase();
        }
        finally
        {
            processTask1.DeleteInDatabase();

            parent1.DeleteInDatabase();
            parent2.DeleteInDatabase();
        }
    }
}