using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.SqlDialogs;


namespace MapsDirectlyToDatabaseTableUI
{
    /// <summary>
    /// This window appears whenever the RDMP has detected that there is a version mismatch between your database and the RDMP software (or a plugin you have written - See PluginPatcherUI).
    /// The RDMP enforces a strict version policy in which the version of the codebase (the software running) must always match that of the databases it is running on.  Each new version of
    /// the RDMP software will include SQL patches designed to bring your databases up-to-date with the new feature set.
    /// 
    /// This dialog shows you the version number of the database and the version number of the patching assembly (separate version numbers are maintained for the Catalogue Manager Database,
    /// Data Export Database, Logging databases etc).  For a description of the Host/Database assembly model the RDMP uses, See ManagedDatabaseUI.
    /// </summary>
    public partial class PatchingUI : Form
    {
        private readonly SqlConnectionStringBuilder _builder;
        private readonly ITableRepository _repository;
        private readonly Version _databaseVersion;
        private readonly Version _hostAssemblyVersion;

        private readonly Assembly _databaseAssembly;
        private readonly Assembly _hostAssembly;

        private readonly Patch[] _patchesInDatabase;
        private readonly SortedDictionary<string, Patch> _allPatchesInAssembly;
        

        private PatchingUI(SqlConnectionStringBuilder builder, ITableRepository repository, Version databaseVersion, Assembly databaseAssembly, Assembly hostAssembly, Patch[] patchesInDatabase, SortedDictionary<string, Patch> allPatchesInAssembly)
        {
            _builder = builder;
            _repository = repository;
            _databaseVersion = databaseVersion;
            _databaseAssembly = databaseAssembly;
            _hostAssembly = hostAssembly;
            
            InitializeComponent();

            if (_hostAssembly == null)
                return;

            _hostAssemblyVersion = new Version(FileVersionInfo.GetVersionInfo(_hostAssembly.Location).FileVersion);
            _patchesInDatabase = patchesInDatabase;
            _allPatchesInAssembly = allPatchesInAssembly;
            
            lblHostAssembly.Text =  hostAssembly.FullName;
            lblPatchingAssembly.Text = databaseAssembly.FullName;


            if (builder == null || string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                lblDatabaseVersion.Text = "Form loaded without a specific database to target!";
                lblDatabaseVersion.ForeColor = Color.Red;
            }
            else
            {
                lblDatabaseVersion.Text = string.Format("{0}, Version:{1}", builder.InitialCatalog,repository.GetVersion());
            }

            

        }

        public static void ShowIfRequired(SqlConnectionStringBuilder builder, ITableRepository repository, Assembly databaseAssembly, Assembly hostAssembly)
        {
            Version databaseVersion;
            Patch[] patchesInDatabase;
            SortedDictionary<string, Patch> allPatchesInAssembly;

            if (Patch.IsPatchingRequired(builder, databaseAssembly, hostAssembly, out databaseVersion, out patchesInDatabase, out allPatchesInAssembly))
                new PatchingUI(builder, repository, databaseVersion, databaseAssembly, hostAssembly, patchesInDatabase, allPatchesInAssembly).ShowDialog();
        }

        private void btnAttemptPatching_Click(object sender, EventArgs e)
        {
            bool stop = false;

            //start with the assumption that we will apply all patches
            SortedDictionary<string, Patch> toApply = new SortedDictionary<string, Patch>();

            foreach (Patch potentialInstallable in _allPatchesInAssembly.Values.Except(_patchesInDatabase))
                toApply.Add(potentialInstallable.locationInAssembly, potentialInstallable);


            try
            {
                //make sure the existing patches in the live database are not freaky phantom patches
                foreach (Patch patch in _patchesInDatabase)
                    //if patch is not in database assembly
                    if (!_allPatchesInAssembly.Any(a => a.Value.Equals(patch)))
                    {

                        checksUI1.OnCheckPerformed(new CheckEventArgs(
                            "The database contains an unexplained patch called " + patch.locationInAssembly +
                            " (it is not in " + _databaseAssembly.FullName + " ) so how did it get there?", CheckResult.Fail,
                            null));
                        stop = true;
                    }
                    else if (!_allPatchesInAssembly[patch.locationInAssembly].EntireScript.Equals(patch.EntireScript))
                    {
                        checksUI1.OnCheckPerformed(new CheckEventArgs(
                            "The contents of patch " + patch.locationInAssembly +
                            " are different between live database and the database patching assembly", CheckResult.Warning,
                            null));

                        //do not apply this patch
                        toApply.Remove(patch.locationInAssembly);
                    }
                    else
                    {
                        //we found it and it was intact
                        checksUI1.OnCheckPerformed(new CheckEventArgs("Patch " + patch.locationInAssembly + " was previously installed successfully so no need to touch it",CheckResult.Success, null));
                    
                        //do not apply this patch
                        toApply.Remove(patch.locationInAssembly);

                    }
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Patch evaluation failed", CheckResult.Fail, exception));
                stop = true;
            }
            

            //if any of the patches we are trying to apply are earlier than the latest in the database
            IEnumerable<Patch> missedOppertunities = toApply.Values.Where(p => p.DatabaseVersionNumber < _patchesInDatabase.Max(p2 => p2.DatabaseVersionNumber));
            foreach (Patch missedOppertunity in missedOppertunities)
            {
                stop = true;
                checksUI1.OnCheckPerformed(new CheckEventArgs(
                    "Patch " + missedOppertunity.locationInAssembly +
                    " cannot be applied because it's version number is " + missedOppertunity.DatabaseVersionNumber +
                    " but the current database is at version " + _databaseVersion
                    + Environment.NewLine
                    + " Contents of patch was:" + Environment.NewLine +missedOppertunity.EntireScript
                    , CheckResult.Fail, null));
            }

            //if the patches to be applied would bring the version number above that of the host Library
            foreach (Patch futurePatch in toApply.Values.Where(patch => patch.DatabaseVersionNumber > _hostAssemblyVersion))
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs(
                    "Cannot apply patch "+futurePatch.locationInAssembly+" because it's database version number is "+futurePatch.DatabaseVersionNumber+" which is higher than the currently loaded host assembly (" +_hostAssembly.FullName+ "). ", CheckResult.Fail, null));
                stop = true;
                
            }


            if (stop)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Abandonning patching process (no patches have been applied) because of one or more previous errors",CheckResult.Fail, null));
                return;
            }
            try
            {
                MasterDatabaseScriptExecutor  executor = new MasterDatabaseScriptExecutor(_builder.ConnectionString);
                executor.PatchDatabase(toApply,checksUI1,PreviewPatch);

                checksUI1.OnCheckPerformed(new CheckEventArgs("Patching completed without exception, disabling the patching button", CheckResult.Success, null));
                //patching worked so prevent them doing it again!
                btnAttemptPatching.Enabled = false;

                if (_repository != null)
                {
                    _repository.ClearUpdateCommandCache();
                    checksUI1.OnCheckPerformed(new CheckEventArgs("Cleared UPDATE commands cache", CheckResult.Success, null));
                }

                checksUI1.OnCheckPerformed(new CheckEventArgs("Patching Succesful", CheckResult.Success, null));

                if (MessageBox.Show("Application will now restart", "Close?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Application.Restart();
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Patching failed", CheckResult.Fail, exception));
            }

        }

        private bool _yesToAll = false;

        private bool PreviewPatch(Patch patch)
        {
            if (_yesToAll)
                return true;

            var preview = new SQLPreviewWindow(patch.locationInAssembly,"The following SQL Patch will be run:", patch.EntireScript);
            try
            {
                return preview.ShowDialog()==DialogResult.OK;
            }
            finally
            {
                _yesToAll = preview.YesToAll;
            }

        }
    }
}
