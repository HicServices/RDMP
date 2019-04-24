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
using CatalogueManager.CommandExecution;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using Tests.Common;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class UITests : UnitTests
    {
        private TestActivateItems _itemActivator;

        protected TestActivateItems ItemActivator
        {
            get
            {
                if (_itemActivator == null)
                {
                    _itemActivator = new TestActivateItems(this,Repository);
                    _itemActivator.RepositoryLocator.CatalogueRepository.MEF = MEF;
                    
                    //if mef was loaded for this test then this is supported otherwise not
                    if(MEF != null)
                        _itemActivator.CommandExecutionFactory = new RDMPCommandExecutionFactory(_itemActivator);
                }

                return _itemActivator;
            }
        }

        

        private ToMemoryCheckNotifier _checkResults;
        private Control _userInterfaceLaunched;
        
        

        /// <summary>
        /// 'Launches' a new instance of the UI defined by Type T which must be compatible with the provided <paramref name="o"/>.  The UI will not
        /// visibly appear but will be mounted on a Form and generally should behave like live ones.
        /// 
        /// <para>Method only tracks one set of results at once, so if you call this method more than once then expect old Errors to disapear.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown when calling this method multiple times within a single test</exception>
        public T AndLaunch<T>(DatabaseEntity o) where T : Control, IRDMPSingleDatabaseObjectControl, new()
        {
            Console.WriteLine("Launched " + typeof(T).Name);

            //clear the old results
            ClearResults();
            
            Form f = new Form();
            T ui = new T();
            f.Controls.Add(ui);
            CreateControls(ui);

            ui.CommonFunctionality.BeforeChecking += CommonFunctionalityOnBeforeChecking;
            ui.CommonFunctionality.OnFatal += CommonFunctionalityOnFatal;
            ui.SetDatabaseObject(ItemActivator, o);

            _userInterfaceLaunched = ui;
            return ui;
        }


        /// <summary>
        /// Clears the ItemActivator and ui fields
        /// </summary>
        [SetUp]
        protected void ClearResults()
        {
            if(_itemActivator != null)
                _itemActivator.Results.Clear();

            _checkResults = null;
            _userInterfaceLaunched = null;
        }
        /// <summary>
        /// Asserts that the given command is impossible for the <paramref name="expectedReason"/>
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="expectedReason">The reason it should be impossible - uses StringAssert.Contains</param>
        protected void AssertCommandIsImpossible(IAtomicCommand cmd, string expectedReason)
        {
            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.Contains(expectedReason, cmd.ReasonCommandImpossible);
        }
        /// <summary>
        /// Asserts that the given command is not marked IsImpossible
        /// </summary>
        /// <param name="cmd"></param>
        protected void AssertCommandIsPossible(IAtomicCommand cmd)
        {
            //if it isn't marked impossible
            if(!cmd.IsImpossible)
                return;

            if(string.IsNullOrWhiteSpace(cmd.ReasonCommandImpossible))
                Assert.Fail("Command was impossible but no reason was given!!!");

            Assert.Fail("Command was Impossible because:" + cmd.ReasonCommandImpossible);
        }
        
        private void CommonFunctionalityOnBeforeChecking(object sender, EventArgs eventArgs)
        {
            //intercept checking and replace with our own in memory checks
            var e = (BeforeCheckingEventArgs) eventArgs;

            _checkResults = new ToMemoryCheckNotifier();
            try
            {
                e.Checkable.Check(_checkResults);
            }
            catch (Exception ex)
            {
                _checkResults.OnCheckPerformed(new CheckEventArgs("Checks threw exception", CheckResult.Fail, ex));
            }
            e.Cancel = true;
        }

        private void CommonFunctionalityOnFatal(object sender, CheckEventArgs e)
        {
            ItemActivator.Results.FatalCalls.Add(e);
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
                    Assert.IsEmpty(ItemActivator.Results.FatalCalls);
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
                    Assert.IsTrue(ItemActivator.Results.KilledForms.Values.Any(v=>v.Message.Contains(expectedContainsText)),"Failed to find expected Exception, Exceptions were:\r\n" +string.Join(Environment.NewLine,ItemActivator.Results.KilledForms.Values.Select(v=>v.ToString())) );
                    break;
                case ExpectedErrorType.Fatal:
                    Assert.IsTrue(ItemActivator.Results.FatalCalls.Any(c => c.Message.Contains(expectedContainsText)));
                    break;
                case ExpectedErrorType.FailedCheck:

                    if (_checkResults == null)
                        throw new Exception("Could not check for Checks error because control did not register an ICheckable");

                    //there must have been something checked that failed with the provided message
                    Assert.IsTrue(_checkResults.Messages.Any(m=>
                        m.Message.Contains(expectedContainsText) || 
                        (m.Ex != null && m.Ex.Message.Contains(expectedContainsText))
                        && m.Result == CheckResult.Fail));

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
            //https://referencesource.microsoft.com/#system.windows.forms/winforms/Managed/System/WinForms/ErrorProvider.cs.html,11db4fca371f280c
            List<string> toReturn = new List<string>();

            var hashtable = (Hashtable) typeof (ErrorProvider).GetField("items",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(errorProvider);

            foreach (var entry in hashtable.Values)
                toReturn.Add((string)entry.GetType().GetField("error", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entry));

            return toReturn;
        }

        private List<ErrorProvider> GetErrorProviders(Control arg)
        {
            List<ErrorProvider> toReturn = new List<ErrorProvider>();


            var errorProviderFields = arg.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(f => f.FieldType == typeof(ErrorProvider));
            
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