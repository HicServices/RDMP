using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Npgsql.Internal;
using Python.Runtime;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Modules.Mutilators
{
    public class MapFreeformTextToCodes : IMutilateDataTables
    {


        [DemandsInitialization("The location of the mapping model")]
        public string ModelLocation { get; set; }

        [DemandsInitialization("The column to extract the mappings from")]
        public string FreeformTextColumn { get; set; }

        [DemandsInitialization("Column to join mapped data on")]
        public string IdentifierColumn { get; set; }

        [DemandsInitialization("Location of your python DLL")]
        public string PythonDLLLocation { get; set; }

        [DemandsInitialization("Location of your python virtual environment", DefaultValue = null)]
        public string PythonVirtualEnvLocation { get; set; } //C:\Users\jfriel001\mednlp\venv

        [DemandsInitialization("Train your local model on the incoming data", DefaultValue = false)]
        public bool TrainModel { get; set; }

        public void Abort(IDataLoadEventListener listener)
        {
        }

        public void Check(ICheckNotifier notifier)
        {
            //throw new NotImplementedException();
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            if (loadStage != LoadStage.PostLoad)
            {
                throw new Exception("Mappings can only be completed during the post-load stage");
            }
            if (!new FileInfo(ModelLocation).Exists)
            {
                throw new Exception("Model location cannot be found");
            }
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            throw new NotImplementedException();
        }

        public ExitCodeType Mutilate(IDataLoadJob job)
        {
            //string pathToVirtualEnv = "C:\\Users\\jfriel001\\mednlp\\venv";
            Runtime.PythonDLL = "C:\\Users\\jfriel001\\AppData\\Local\\Programs\\Python\\Python313\\Python313.dll";
            PythonEngine.Initialize();
            //PythonEngine.PythonHome = pathToVirtualEnv;
            //PythonEngine.PythonPath = PythonEngine.PythonPath + ";" + $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib";


            foreach (var table in job.RegularTablesToLoad)
            {
                var discoveredTable = table.Discover(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad);
                var sql = $"SELECT {table.ColumnInfos.First(ci => ci.GetRuntimeName() == FreeformTextColumn).GetFullyQualifiedName()},{table.ColumnInfos.First(ci => ci.GetRuntimeName() == IdentifierColumn).GetFullyQualifiedName()} FROM{table.GetFullyQualifiedName()}";// WHERE {SpecialFieldNames.DataLoadRunID} = {job.DataLoadInfo.ID}";
                var connection = discoveredTable.Database.Server.GetConnection() as SqlConnection;
                connection.Open();
                var dt = new DataTable();
                SqlCommand cmd = new SqlCommand(sql, connection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                using (Py.GIL())
                {
                    dynamic cat;
                    try
                    {
                        var Scope = Py.CreateScope();
                        PyObject script = PythonEngine.Compile("from medcat.cat import CAT");
                        Scope.Execute(script);
                        var result = Scope.Get("CAT");

                        cat = result.InvokeMethod("load_model_pack", ModelLocation.ToPython());
                    }
                    catch (Exception e)
                    {
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, e.Message));
                        return ExitCodeType.Error;
                    }
                    //dynamic medcat = Py.Import("medcat.cat");
                    //dynamic cat = medcat.CAT.load_model_pack(ModelLocation);
                    foreach (var row in dt.Rows.Cast<DataRow>())
                    {
                        PyDict entities = cat.get_entities(row.ItemArray[0]);
                        string values = entities["entities"].ToString();

                        var x = entities["entities"];//class dict??
                        //PyDict d = entities.;
                        Console.WriteLine('w');
                        //foreach (PyObject value in entities.Values())
                        //{
                        //    var x = value.AsManagedObject(typeof(PyDict));
                        //    Console.WriteLine('w');
                        //var prettyName = value.GetAttr("pretty_name");
                        //    var cui = value.GetAttr("cui");

                        //    //EntityResult result = (EntityResult)value.AsManagedObject(typeof(EntityResult));
                        //    //var prettyName = value["pretty_name"];
                        //    //var cui = value["cui"];
                        //    //var typeIds = value["type_ids"];//list
                        //    //var types = value["types"];//list
                        //    //var sourceValue = value["source_value"];
                        //    //var detected_name = value["detected_name"];
                        //    //var acc = value["acc"];
                        //    //var contextSimilarity = value["context_similarity"];
                        //    //var start = value["start"];
                        //    //var end = value["end"];
                        //    //var icd10= value["icd10"];//list
                        //    //var ontologies = value["ontologies"];//list
                        //    //var snomed = value["snomed"];//list
                        //    //var metaAnalysis = value["meta_anns"];//dict



                        //    //  {
                        //    //              'pretty_name': 'Diphtheria',
                        //    //'cui': 'C0012546',
                        //    //'type_ids': ['T047'],
                        //    //'types': ['Disease or Syndrome'],
                        //    //'source_value': 'Diphtheria',
                        //    //'detected_name': 'diphtheria',
                        //    //'acc': 0.2505741371065839,
                        //    //'context_similarity': 0.2505741371065839,
                        //    //'start': 14,
                        //    //'end': 24,
                        //    //'icd10': [],
                        //    //'ontologies': [],
                        //    //'snomed': [],
                        //    //'id': 1,
                        //    //'meta_anns': {
                        //    //                  'Status': {
                        //    //                      'value': 'Other',
                        //    //                         'confidence': 0.9998244643211365,
                        //    //                         'name': 'Status'}
                        //    //              }
                        //    //          }
                        //    Console.WriteLine('w');
                        //}
                        //}
                    }
                }
            }

            return ExitCodeType.Success;
        }
        private class EntityResult
        {
            public string pretty_name { get; set; }
            public string cui { get; set; }
        }

    }

}
