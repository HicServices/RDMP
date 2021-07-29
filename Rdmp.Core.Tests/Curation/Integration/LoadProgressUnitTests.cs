﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling.Strategy;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.IO;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class LoadProgressUnitTests : UnitTests
    {
        [Test]
        public void LoadProgress_Checks_BadDates()
        {
            var lp = WhenIHaveA<LoadProgress>();

            lp.Check(new ThrowImmediatelyCheckNotifier());

            //Bad Origin Date
            lp.OriginDate = DateTime.Now.AddDays(1);
            Assert.Throws<Exception>(()=>lp.Check(new ThrowImmediatelyCheckNotifier()));

            //Back to normal
            lp.RevertToDatabaseState();
            lp.Check(new ThrowImmediatelyCheckNotifier());

            //Bad ProgressDate
            lp.DataLoadProgress = DateTime.Now.AddDays(1);
            Assert.Throws<Exception>(()=>lp.Check(new ThrowImmediatelyCheckNotifier()));
                        
            //Back to normal
            lp.RevertToDatabaseState();
            lp.Check(new ThrowImmediatelyCheckNotifier());

            // valid progress (1 year)
            lp.OriginDate = new DateTime(2001,1,1);
            lp.DataLoadProgress = new DateTime(2002,1,1);
            lp.Check(new ThrowImmediatelyCheckNotifier());
        }

        [Test]
        public void LoadProgress_JobFactory_NoDates()
        {
            var lp = WhenIHaveA<LoadProgress>();

            

            lp.OriginDate = new DateTime(2001,1,1);
            
            // We are fully up-to-date
            lp.DataLoadProgress = DateTime.Now;
            
            lp.Check(new ThrowImmediatelyCheckNotifier());
            
            var stratFactory = new JobDateGenerationStrategyFactory(new AnyAvailableLoadProgressSelectionStrategy(lp.LoadMetadata));
            var strat = stratFactory.Create(lp,new ThrowImmediatelyDataLoadEventListener());
            
            var dir = LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.WorkDirectory),"LoadProgress_JobFactory_NoDates",true);
            
            var lmd = lp.LoadMetadata;
            lmd.LocationOfFlatFiles = dir.RootPath.FullName;
            
            foreach(var cata in lmd.GetAllCatalogues())
            {
                cata.LoggingDataTask = "ff";
                cata.SaveToDatabase();
            }
                

            lmd.SaveToDatabase();
            

            var jobFactory = new SingleScheduledJobFactory(lp,strat,999,lp.LoadMetadata,null);
            var ex = Assert.Throws<Exception>(()=>jobFactory.Create(RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(),null));

            Assert.AreEqual("DatesToRetrieve was empty for load 'MyLoad'.  Possibly the load is already up to date?",ex.Message);

            // We have 1 day to load (date is the last fully loaded date)
            lp.DataLoadProgress = DateTime.Now.AddDays(-2);
            lp.SaveToDatabase();
             
            strat = stratFactory.Create(lp,new ThrowImmediatelyDataLoadEventListener());
            jobFactory =  new SingleScheduledJobFactory(lp,strat,999,lp.LoadMetadata,null);

            var job = jobFactory.Create(RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(),null);
            Assert.AreEqual(1,((ScheduledDataLoadJob)job).DatesToRetrieve.Count);
        }
    }
}
