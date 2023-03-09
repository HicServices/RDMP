// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit
{
    class CohortSamplerTests : UnitTests
    {
        [Test]
        public void TestCohortSampler_NoColumnFound()
        {
            var sampler = GetCohortSampler();

            var dt = new DataTable();
            dt.Columns.Add("ff");
            dt.Rows.Add("1");

            var ex = Assert.Throws<Exception>(()=>sampler.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
            Assert.AreEqual("CohortSampler was unable to find a column called 'priv' in the data passed in.  This is the expected private identifier column name of the cohort you are committing.", ex.Message);
        }

        [Test]
        public void TestCohortSampler_NoColumnFoundExplicit()
        {
            var sampler = GetCohortSampler();
            sampler.PrivateIdentifierColumnName = "ddd";

            var dt = new DataTable();
            dt.Columns.Add("ff");
            dt.Rows.Add("1");

            var ex = Assert.Throws<Exception>(() => sampler.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
            Assert.AreEqual("CohortSampler was unable to find a column called 'ddd' in the data passed in.  This is the expected private identifier column name of the cohort you are committing.", ex.Message);
        }
        [Test]
        public void TestCohortSampler_NotEnoughIdentifiersInBatch()
        {
            var sampler = GetCohortSampler();
            sampler.SampleSize = 100;

            var dt = new DataTable();
            dt.Columns.Add("priv");
            dt.Rows.Add("1");

            var ex = Assert.Throws<Exception>(() => sampler.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
            Assert.AreEqual("Cohort only contains 1 unique identifiers.  This is less than the requested sample size of 100 and FailIfNotEnoughIdentifiers is true", ex.Message);
        }
        [Test]
        public void TestCohortSampler_NotEnoughIdentifiersInBatch_ButThatsOk()
        {
            var sampler = GetCohortSampler();
            sampler.SampleSize = 100;
            // accept less than 100
            sampler.FailIfNotEnoughIdentifiers = false;

            var dt = new DataTable();
            dt.Columns.Add("priv");
            dt.Rows.Add("1");

            var result = sampler.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            Assert.AreEqual(1, result.Rows.Count);
        }

        [Test]
        public void TestCohortSampler_Repeatability()
        {
            var sampler1 = GetCohortSampler();
            var sampler2 = GetCohortSampler();

            sampler1.SampleSize = 5;
            sampler2.SampleSize = 5;

            var dt = new DataTable();
            dt.Columns.Add("priv");
            dt.Rows.Add("11");
            dt.Rows.Add("2123");
            dt.Rows.Add("32213");
            dt.Rows.Add("41");
            dt.Rows.Add("14515");
            dt.Rows.Add("13516");
            dt.Rows.Add("12517");
            dt.Rows.Add("1245121");
            dt.Rows.Add("432143211");
            dt.Rows.Add("12341241");
            dt.Rows.Add("1121234");
            dt.Rows.Add("612346231");
            dt.Rows.Add("62323616");
            dt.Rows.Add("2362361");

            var result1 = sampler1.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            
            // just to be sure
            Thread.Sleep(100);

            var result2 = sampler2.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.AreEqual(5, result1.Rows.Count);
            Assert.AreEqual(5, result2.Rows.Count);


            Assert.IsTrue(result1.Rows.Cast<DataRow>().Select(r => r[0])
                .SequenceEqual(result2.Rows.Cast<DataRow>().Select(r => r[0])),
                "Expected both samplers to select the same random sample of input values because the project number is the seed");
        }


        [Test]
        public void TestCohortSampler_Repeatability_OrderIrrelevant()
        {
            var sampler1 = GetCohortSampler();
            var sampler2 = GetCohortSampler();

            sampler1.SampleSize = 2;
            sampler2.SampleSize = 2;

            var dt1 = new DataTable();
            dt1.Columns.Add("priv");
            dt1.Rows.Add("11");
            dt1.Rows.Add("2123");
            dt1.Rows.Add("32213");
            dt1.Rows.Add("asdf");
            dt1.Rows.Add("hgreerg");

            var dt2 = new DataTable();
            dt2.Columns.Add("priv");
            dt2.Rows.Add("asdf");
            dt2.Rows.Add("11");
            dt2.Rows.Add("hgreerg");
            dt2.Rows.Add("32213");
            dt2.Rows.Add("2123");

            var result1 = sampler1.ProcessPipelineData(dt1, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            // just to be sure
            Thread.Sleep(100);

            var result2 = sampler2.ProcessPipelineData(dt2, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.AreEqual(2, result1.Rows.Count);
            Assert.AreEqual(2, result2.Rows.Count);


            Assert.IsTrue(result1.Rows.Cast<DataRow>().Select(r => r[0])
                .SequenceEqual(result2.Rows.Cast<DataRow>().Select(r => r[0])),
                "Expected both samplers to select the same random sample of input values because the project number is the seed");
        }

        private CohortSampler GetCohortSampler()
        {
            var sampler = new CohortSampler();

            var definition = new CohortDefinition(null, "my cool cohort", 1, 234, WhenIHaveA<ExternalCohortTable>());
            var p = WhenIHaveA<Project>();
            p.ProjectNumber = 234;

            var request = new CohortCreationRequest(p, definition, Repository, "Cohort read from space!!!");

            sampler.PreInitialize(request, new ThrowImmediatelyDataLoadEventListener());

            return sampler;
        }
    }
}