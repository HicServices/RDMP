// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Repositories;
using FAnsi.Implementation;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class UITests
    {
        protected MemoryDataExportRepository Repository = new MemoryDataExportRepository();
        protected TestActivateItems ItemActivator;

        protected MEF MEF;

        private ToMemoryCheckNotifier _checkResults;
        private Control _userInterfaceLaunched;

        /// <summary>
        /// Call if your test needs to access classes via MEF.  Loads all dlls in the test directory.
        /// 
        /// <para>This must be called before you 'launch' your ui</para>
        /// </summary>
        protected void SetupMEF()
        {
            MEF = new MEF();
            MEF.Setup(new SafeDirectoryCatalog(TestContext.CurrentContext.TestDirectory));
        }
        
        /// <summary>
        /// Creates a minimum viable object of Type T.  This includes the object and any dependencies e.g. a 
        /// <see cref="ColumnInfo"/> cannot exist without a <see cref="TableInfo"/>.  
        /// </summary>
        /// <typeparam name="T">Type of object you want to create</typeparam>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">If there is not yet an implementation for the given T.  Feel free to write one.</exception>
        protected T WhenIHaveA<T>() where T:DatabaseEntity
        {
            T toReturn = null;

            if (typeof (T) == typeof (Catalogue))
                return (T)(object) Save(new Catalogue(Repository, "Mycata"));

            if (typeof(T) == typeof(CatalogueItem))
            {
                var cata = new Catalogue(Repository, "Mycata");
                return (T)(object)Save(new CatalogueItem(Repository, cata, "MyCataItem"));
            }

            if (typeof(T) == typeof(ExtractionInformation))
            {
                var col = WhenIHaveA<ColumnInfo>();

                var cata = new Catalogue(Repository, "Mycata");
                var ci = new CatalogueItem(Repository, cata, "MyCataItem");
                var ei = new ExtractionInformation(Repository, ci, col, "MyCataItem");
                return (T)(object)Save(ei);
            }

            if (typeof (T) == typeof (TableInfo))
            {
                var table = new TableInfo(Repository, "My_Table");
                return  (T)(object)Save(table);
            }

            if (typeof (T) == typeof (ColumnInfo))
            {
                var ti = WhenIHaveA<TableInfo>();
                var col = new ColumnInfo(Repository,"My_Col","varchar(10)",ti);
                return (T)(object)Save(col);
            }

            if (typeof (T) == typeof (AggregateConfiguration))
            {
                ExtractionInformation dateEi;
                ExtractionInformation otherEi;
                return (T)(object)WhenIHaveA<AggregateConfiguration>(out dateEi, out otherEi);
            }

            if (typeof (T) == typeof (ExternalDatabaseServer))
            {
                return (T) (object) Save(new ExternalDatabaseServer(Repository,"My Server"));
            }

            if (typeof (T) == typeof (ANOTable))
            {
                ExternalDatabaseServer server;
                return (T)WhenIHaveA<ANOTable>(out server);
            }

            throw new NotSupportedException();
        }

        /// <inheritdoc cref="WhenIHaveA{T}()"/>
        protected AggregateConfiguration WhenIHaveA<T>(out ExtractionInformation dateEi, out ExtractionInformation otherEi) where T : AggregateConfiguration
        {
            var ti = WhenIHaveA<TableInfo>();
            var dateCol = new ColumnInfo(Repository, "MyDateCol", "datetime2", ti);
            var otherCol = new ColumnInfo(Repository, "MyOtherCol", "varchar(10)", ti);

            var cata = WhenIHaveA<Catalogue>();
            var dateCi = new CatalogueItem(Repository, cata, dateCol.Name);
            dateEi = new ExtractionInformation(Repository, dateCi, dateCol, dateCol.Name);
            var otherCi = new CatalogueItem(Repository, cata, otherCol.Name);
            otherEi = new ExtractionInformation(Repository, otherCi, otherCol, otherCol.Name);
            return Save(new AggregateConfiguration(Repository, cata, "My graph"));
        }
        
        /// <inheritdoc cref="WhenIHaveA{T}()"/>
        protected DatabaseEntity WhenIHaveA<T>(out ExternalDatabaseServer server) where T:ANOTable
        {
            server = new ExternalDatabaseServer(Repository, "ANO Server", typeof(ANOStore.Database.Class1).Assembly);
            var anoTable = new ANOTable(Repository, server, "ANOFish", "F");
            return anoTable;
        }

        private T Save<T>(T s) where T:ISaveable
        {
            s.SaveToDatabase();
            return s;
        }

        /// <summary>
        /// 'Launches' a new instance of the UI defined by Type T which must be compatible with the provided <paramref name="o"/>.  The UI will not
        /// visibly appear but will be mounted on a Form and generally should behave like live ones.
        /// 
        /// <para>Test should only call this method once.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown when calling this method multiple times within a single test</exception>
        protected T AndLaunch<T>(DatabaseEntity o) where T : Control, IRDMPSingleDatabaseObjectControl, new()
        {
            if (_userInterfaceLaunched != null)
                throw new NotSupportedException("AndLaunch called multiple times");

            if (ItemActivator == null)
            {
                ItemActivator = new TestActivateItems(Repository);
                ItemActivator.RepositoryLocator.CatalogueRepository.MEF = MEF;
            }
            
            Form f = new Form();
            T ui = new T();
            f.Controls.Add(ui);
            CreateControls(ui);
            ui.SetDatabaseObject(ItemActivator, o);
            ui.CommonFunctionality.BeforeChecking += CommonFunctionalityOnBeforeChecking;
            _userInterfaceLaunched = ui;
            return ui;
        }


        /// <summary>
        /// Loads FAnsi implementations for all supported DBMS platforms into memory
        /// </summary>
        [SetUp]
        protected void LoadDatabaseImplementations()
        {
            ImplementationManager.Load(
                typeof(FAnsi.Implementations.MicrosoftSQL.MicrosoftSQLImplementation).Assembly,
                typeof(FAnsi.Implementations.MySql.MySqlImplementation).Assembly,
                typeof(FAnsi.Implementations.Oracle.OracleImplementation).Assembly);

            if(ItemActivator != null)
                ItemActivator.Results.Clear();

            _checkResults = null;
            _userInterfaceLaunched = null;
        }

        /// <summary>
        /// Asserts that the given command is impossible for the <paramref name="expectedReason"/>
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="expectedReason">The reason it should be impossible - uses StringAssert.Contains</param>
        protected void AssertImpossibleBecause(IAtomicCommand cmd, string expectedReason)
        {
            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.Contains(expectedReason, cmd.ReasonCommandImpossible);
        }
        
        private void CommonFunctionalityOnBeforeChecking(object sender, EventArgs eventArgs)
        {
            //intercept checking and replace with our own in memory checks
            var e = (BeforeCheckingEventArgs) eventArgs;

            _checkResults = new ToMemoryCheckNotifier();
            e.Checkable.Check(_checkResults);
            e.Cancel = true;

        }

        /// <summary>
        /// Checks the recorded errors up to this point in the test and fails the test if there are errors
        /// at the given <paramref name="expectedErrorLevel"/>
        /// </summary>
        /// <param name="expectedErrorLevel"></param>
        protected void AssertNoErrors(ExpectedErrorType expectedErrorLevel)
        {
            switch (expectedErrorLevel)
            {
                case ExpectedErrorType.KilledForm:
                    Assert.IsEmpty(ItemActivator.Results.KilledForms);
                    break;
                case ExpectedErrorType.Fatal:
                    break;
                case ExpectedErrorType.FailedCheck:
                    
                    //there must have been something checked that failed with the provided message
                    if (_checkResults != null)
                        Assert.IsEmpty(_checkResults.Messages.Where(m => m.Result == CheckResult.Fail));
                    break;
                case ExpectedErrorType.ErrorProvider:
                    Assert.IsEmpty(GetAllErrorProviderErrorsShown());
                    
                    break;
                case ExpectedErrorType.Any:
                    AssertNoErrors(ExpectedErrorType.KilledForm);
                    AssertNoErrors(ExpectedErrorType.Fatal);
                    AssertNoErrors(ExpectedErrorType.FailedCheck);
                    AssertNoErrors(ExpectedErrorType.ErrorProvider);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("expectedErrorLevel");
            }
        }

        /// <summary>
        /// Checks that the given <paramref name="expectedContainsText"/> was displayed to the user at the 
        /// given <paramref name="expectedErrorLevel"/>
        /// </summary>
        /// <param name="expectedErrorLevel">The error level to be checked.  Do not pass 'Any'</param>
        /// <param name="expectedContainsText"></param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="expectedErrorLevel"/> is not supported e.g. Any</exception>
        protected void AssertErrorWasShown(ExpectedErrorType expectedErrorLevel, string expectedContainsText)
        {
            switch (expectedErrorLevel)
            {
                case ExpectedErrorType.KilledForm:
                    Assert.IsTrue(ItemActivator.Results.KilledForms.Values.Any(v=>v.Message.Contains(expectedContainsText)));
                    break;
                case ExpectedErrorType.Fatal:
                    break;
                case ExpectedErrorType.FailedCheck:

                    //there must have been something checked that failed with the provided message
                    Assert.IsTrue(_checkResults.Messages.Any(m=>m.Message.Contains(expectedContainsText) && m.Result == CheckResult.Fail));

                    break;
                case ExpectedErrorType.ErrorProvider:

                    Assert.IsTrue(GetAllErrorProviderErrorsShown().Any(m => m.Contains(expectedContainsText)));

                    break;
                default:
                    throw new ArgumentOutOfRangeException("expectedErrorLevel");
            }
        }
        private List<string> GetAllErrorProviderErrorsShown()
        {

            var errorProviders =
                //get all controls with ErrorProvider fields
                GetControl<Control>().SelectMany(GetErrorProviders)
                //and any we registered through the BinderWithErrorProviderFactory
                .Union(ItemActivator.Results.RegisteredRules.Select(r => r.ErrorProvider))
                .ToList();

            //get the error messages that have been shown from any of these
            return errorProviders.SelectMany(GetErrorProviderErrorsShown).ToList();
        }

        private List<string> GetErrorProviderErrorsShown(ErrorProvider errorProvider)
        {
            List<string> toReturn = new List<string>();

            var hashtable = (Hashtable) typeof (ErrorProvider).GetField("items",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(errorProvider);

            foreach (var entry in hashtable.Values)
                toReturn.Add((string)entry.GetType().GetField("error", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entry));

            return toReturn;
        }

        private List<ErrorProvider> GetErrorProviders(Control arg)
        {
            List<ErrorProvider> toReturn = new List<ErrorProvider>();

            
            var errorProviderFields = arg.GetType().GetFields().Where(f => f.FieldType == typeof (ErrorProvider));
            
            foreach (FieldInfo f in errorProviderFields)
            {
                var instance = f.GetValue(arg);
                if(instance != null)
                    toReturn.Add((ErrorProvider)instance);
            }

            return toReturn;
        }

        /// <summary>
        /// Returns all controls of type T that are in the currently shown user interface 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected List<T> GetControl<T>() where T:Control
        {
            return GetControl<T>(_userInterfaceLaunched, new List<T>());
        }

        private List<T> GetControl<T>(Control c, List<T> list) where T:Control
        {
            if(c is T)
                list.Add((T)c);

            foreach (Control child in c.Controls)
                GetControl(child, list);

            return list;
        }

        /// <summary>
        /// Triggers a refresh message for the given <paramref name="o"/>.  This will update any user interfaces displaying it.
        /// </summary>
        /// <param name="o"></param>
        protected void Publish(DatabaseEntity o)
        {
            ItemActivator.RefreshBus.Publish(this, new RefreshObjectEventArgs(o));
        }

        private static void CreateControls(Control control)
        {
            CreateControl(control);
            foreach (Control subcontrol in control.Controls)
            {
                CreateControl(subcontrol);
            }
        }
        private static void CreateControl(Control control)
        {
            var method = control.GetType().GetMethod("CreateControl", BindingFlags.Instance | BindingFlags.NonPublic);
            var parameters = method.GetParameters();
            Debug.Assert(parameters.Length == 1, "Looking only for the method with a single parameter");
            Debug.Assert(parameters[0].ParameterType == typeof(bool), "Single parameter is not of type boolean");

            method.Invoke(control, new object[] { true });
        }
    }

    public enum ExpectedErrorType
    {
        /// <summary>
        /// Form must have been made a request to be forceably closed (this is the highest level of error a form can instigate).
        /// </summary>
        KilledForm,

        /// <summary>
        /// Form decided to notify user of a problem outside the scope of the object ICheckable
        /// </summary>
        Fatal,

        /// <summary>
        /// ICheckable object was checked and failed checks in the UI
        /// </summary>
        FailedCheck,

        /// <summary>
        /// An ErrorProvider icon was shown next to some control
        /// </summary>
        ErrorProvider,

        /// <summary>
        /// An error at any level
        /// </summary>
        Any
    }
}