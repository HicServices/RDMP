// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.LoadExecutionUIs;
using CatalogueManager.MainFormUITabs;
using CohortManager.SubComponents;
using Dashboard.Raceway;
using DataExportManager.ProjectUI;
using NUnit.Framework;

namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    public class SinglePropertyUISourceCodeEvaluator
    {
        private string[] PermissableFreakyFileList = new[]
        {
            "FilterUIToParameterCollectionUIToParameterSetUISwitcherPanel",
            "ImportSQLTable",
            "ConfigureDatasetUI" //<== a project extraction configuration + a dataset

        };

        public void FindProblems(List<string> csFilesFound)
        {
            int goodClasses = 0;
            List<string> problems = new List<string>();

            Console.WriteLine("//////////////////////////////////Evalating Public Properties On Controls///////////////////////////////");

            foreach (Type t in GetTypes())
            {
                if (typeof (Control).IsAssignableFrom(t))
                {
                    //it's assignable from control
                    var publicDatabaseEntityProperties = t.GetProperties().Where(p => typeof(DatabaseEntity).IsAssignableFrom(p.PropertyType) &&

                        //This is IRDMPSingleDatabaseObjectControl
                        !p.Name.Equals("DatabaseObject")
                        
                        && p.GetSetMethod() != null
                        ).ToArray();

                    if(publicDatabaseEntityProperties.Length == 0)
                        continue;

                    //it's freaky but permissable

                    if (PermissableFreakyFileList.Contains(t.Name))
                    {
                        Console.WriteLine("Good Class:" + t.Name + "(Permissable Freaky File)");
                        goodClasses++;
                        continue;
                    }

                    if(publicDatabaseEntityProperties.Length == 1)
                    {
                        Console.WriteLine("Good Class:" +t.Name + "(" +publicDatabaseEntityProperties[0].Name+")");
                        goodClasses++;
                    }

                    if (publicDatabaseEntityProperties.Length > 1)
                    {
                        var strProblem = "FAIL:Class " + t.Name + " has " + publicDatabaseEntityProperties.Length + " public properties " + string.Join(",", publicDatabaseEntityProperties.Select(p => p.Name));
                        Console.WriteLine(strProblem);
                        problems.Add(strProblem);
                    }
                }
            }
            
            Assert.AreEqual(0,problems.Count,"Bad Classes:" + problems.Count + "/" + (goodClasses + problems.Count));
        }

        public IEnumerable<Type> GetTypes()
        {
            List<Type> toReturn = new List<Type>();

            toReturn.AddRange(typeof (CatalogueUI).Assembly.GetTypes());
            toReturn.AddRange(typeof(ProjectUI).Assembly.GetTypes());
            toReturn.AddRange(typeof(RacewayRenderAreaUI).Assembly.GetTypes());
            toReturn.AddRange(typeof(CohortCompilerUI).Assembly.GetTypes());

            return toReturn;
        }
    }
}