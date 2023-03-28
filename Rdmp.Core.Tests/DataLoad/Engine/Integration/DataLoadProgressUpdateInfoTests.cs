// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.DataLoad.Modules.LoadProgressUpdating;
using Rdmp.Core.Logging;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class DataLoadProgressUpdateInfoTests :DatabaseTests
{
    private ScheduledDataLoadJob _job;

    #region Setup Methods
    public DataLoadProgressUpdateInfoTests()
    {
        ICatalogue cata = Mock.Of<ICatalogue>(
            c=> c.LoggingDataTask == "NothingTask" && 
                c.GetTableInfoList(false) == Array.Empty<TableInfo>() &&
                c.GetLookupTableInfoList() == Array.Empty<TableInfo>());
            
        var lmd = Mock.Of<ILoadMetadata>(m => m.GetAllCatalogues() == new[] { cata });

        _job = new ScheduledDataLoadJob(null,"fish", Mock.Of<ILogManager>(), lmd, null, new ThrowImmediatelyDataLoadJob(),null);
    }
    #endregion

    [Test]
    public void AddBasicNormalStrategy_NoDates()
    {
        var updateInfo = new DataLoadProgressUpdateInfo();
        var ex = Assert.Throws<DataLoadProgressUpdateException>(()=>updateInfo.AddAppropriateDisposeStep(_job,null));
        Assert.IsTrue(ex.Message.StartsWith("Job does not have any DatesToRetrieve"));
    }

    [Test]
    public void AddBasicNormalStrategy_MaxDate()
    {
        var updateInfo = new DataLoadProgressUpdateInfo();
        Assert.AreEqual(DataLoadProgressUpdateStrategy.UseMaxRequestedDay,updateInfo.Strategy);

        _job.DatesToRetrieve = new List<DateTime>();
        _job.DatesToRetrieve.Add(new DateTime(2001,1,1));
        _job.DatesToRetrieve.Add(new DateTime(2001, 1, 2));
        _job.DatesToRetrieve.Add(new DateTime(2001, 1, 3));
        try
        {
            var added = (UpdateProgressIfLoadsuccessful)updateInfo.AddAppropriateDisposeStep(_job, null);
                
                
            Assert.AreEqual(new DateTime(2001, 1, 3), added.DateToSetProgressTo);
        }
        finally
        {
            _job.DatesToRetrieve.Clear();    
        }
            
    }

    [Test]
    public void AddRAWSQLStrategy_NoSQL()
    {
        var updateInfo = new DataLoadProgressUpdateInfo();
        updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            
        var ex = Assert.Throws<Exception>(()=>updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)));

        Assert.IsTrue(ex.Message.StartsWith("Strategy is ExecuteScalarSQLInRAW but there is no ExecuteScalarSQL"));
    }

    [Test]
    public void AddRAWSQLStrategy_SQLDodgy_SqlCrashes()
    {
        var updateInfo = new DataLoadProgressUpdateInfo();
        updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            
        updateInfo.ExecuteScalarSQL = "SELECT Top 1 BarrelORum from CaptainMorgansSpicedRumBarrel";
        var ex = Assert.Throws<DataLoadProgressUpdateException>(() => updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)));

        Assert.IsTrue(ex.Message.StartsWith("Failed to execute the following SQL in the RAW database"));
        Assert.IsInstanceOf<SqlException>(ex.InnerException);
    }

    [Test]
    public void AddRAWSQLStrategy_SQLDodgy_SqlReturnsNull()
    {
        var updateInfo = new DataLoadProgressUpdateInfo();
        updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            
        updateInfo.ExecuteScalarSQL = "SELECT null";
        var ex = Assert.Throws<DataLoadProgressUpdateException>(() => updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)));

        Assert.IsTrue(ex.Message.Contains("ExecuteScalarSQL"));
        Assert.IsTrue(ex.Message.Contains("returned null"));
    }

    [Test]
    public void AddRAWSQLStrategy_SQLDodgy_SqlReturnsNonDate()
    {
        var updateInfo = new DataLoadProgressUpdateInfo();
        updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            
        updateInfo.ExecuteScalarSQL = "SELECT 'fishfish'";
        var ex = Assert.Throws<DataLoadProgressUpdateException>(() => updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)));

        Assert.AreEqual("ExecuteScalarSQL specified for determining the maximum date of data loaded returned a value that was not a Date:fishfish",ex.Message);
        Assert.IsInstanceOf<FormatException>(ex.InnerException);
    }

    [Test]
    public void AddRAWSQLStrategy_SQLCorrect()
    {
        _job.DatesToRetrieve = new List<DateTime>();
        _job.DatesToRetrieve.Add(new DateTime(2001,1,6));
        _job.DatesToRetrieve.Add(new DateTime(2001,1,7));
        _job.DatesToRetrieve.Add(new DateTime(2001,1,8));

        var updateInfo = new DataLoadProgressUpdateInfo();
        updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
        updateInfo.ExecuteScalarSQL = "SELECT '2001-01-07'";

        var added = (UpdateProgressIfLoadsuccessful)updateInfo.AddAppropriateDisposeStep(_job, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer));
            
        Assert.AreEqual(new DateTime(2001, 1, 7), added.DateToSetProgressTo);

        _job.DatesToRetrieve.Clear();
    }
}