using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.MainFormUITabs;
using CohortManager.SubComponents;
using Dashboard.Raceway;
using DataExportManager.ProjectUI;
using DatasetLoaderUI;
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

            toReturn.AddRange(typeof (CatalogueTab).Assembly.GetTypes());
            toReturn.AddRange(typeof(ProjectUI).Assembly.GetTypes());
            toReturn.AddRange(typeof(DatasetLoadControl).Assembly.GetTypes());
            toReturn.AddRange(typeof(RacewayRenderAreaUI).Assembly.GetTypes());
            toReturn.AddRange(typeof(CohortCompilerUI).Assembly.GetTypes());

            return toReturn;
        }
    }
}