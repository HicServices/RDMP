using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Providers;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.AutoComplete;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode;
using ReusableUIComponents;

using ReusableUIComponents.ScintillaHelper;
using ReusableUIComponents.SingleControlForms;
using ScintillaNET;


namespace CatalogueManager.ExtractionUIs
{
    /// <summary>
    /// The RDMP is intended to curate research datasets, including which recording which columns in a given dataset are extractable and the Governance level for those columns (e.g. can
    /// anyone get the column or are special approvals required).  This window lets you decide whether a CatalogueItem is extractable, optionally provide a transform (e.g. UPPER() etc),
    /// curate filter logic, flag it as the datasets extraction identifier (e.g. PatientId).
    /// 
    /// Start by deciding whether a given Column is extractable by ticking Yes or No.  Then choose an extraction category, a column will only appear in DataExportManager as extractable if
    /// it is Core, Supplemental or SpecialApproval (Internal and Deprecated columns cannot be extracted).  
    /// 
    /// You should have a single field across all your datasets which identifies your cohorts (patients) e.g. PatientId.  If this column contains PatientIds then tick 'Is Extraction 
    /// Identifier', very occasionally you might have multiple columns containing PatientIds e.g. Birth records might have a column for MotherId and a column for BabyId both of which contain
    /// PatientIds (if this is the case then just tick both as 'Is Extraction Identifier'.  
    /// 
    /// You can edit the Extraction Code which is a single line of SELECT SQL.  If you change this to include a function or something else make sure to include an alias 
    /// (e.g. 'UPPER(MyTable.MyColumn) as MyColumn')
    /// 
    /// You can also view the Filters that are associated with this column.  These are centrally curated and validated (Make sure to validate your filters!!!) pieces of WHERE logic which
    /// can be used in Data Extraction and Cohort Identification with the dataset.  For example the Prescribing.DrugCode column could have 2 filters 'Prescription Painkillers' and 
    /// 'Diabetes Drugs'.  Filters should be adequately documented with name and description such that a data analyst can use them without necessarily understanding the SQL implementation.
    /// For more information on configuring Filters see ExtractionFilterUI.
    /// 
    /// If you tick the HashOnDataRelease column then the transform/column will be wrapped by the Hashing Algorithm (if any - See ConfigureHashingAlgorithm) when it comes to data extraction.
    /// Use this only if you have a hashing system implemented.  Hashing is separate from identifier allocation such as ANO (See ConfigureANOForTableInfo) in that its done at extraction
    /// time in SQL only and the exact implementation is up to you.
    ///
    /// 
    /// </summary>
    public partial class ExtractionInformationUI : ExtractionInformationUI_Design, IConsultableBeforeClosing, ISaveableUI
    {
        public ExtractionInformation ExtractionInformation { get; private set; }
        
        //Editor that user can type into
        private Scintilla QueryEditor;

        //handles the case when the user renames the SQL e.g. by putting an alias on the column
        private bool _namesMatchedWhenDialogWasLaunched = false;

        private bool isFirstTimeSetupCalled = true;

        public ExtractionInformationUI()//For use with SetDatabaseObject
        {
            InitializeComponent();
            
            if (VisualStudioDesignMode) //stop right here if in designer mode
                return;

            //note that we don't add the Any category
            ddExtractionCategory.DataSource = new object[] { ExtractionCategory.Core, ExtractionCategory.Supplemental, ExtractionCategory.SpecialApprovalRequired, ExtractionCategory.Internal,ExtractionCategory.Deprecated};
            QueryEditor = new ScintillaTextEditorFactory().Create(new RDMPCommandFactory());
            QueryEditor.TextChanged += QueryEditorOnTextChanged;

            objectSaverButton1.BeforeSave += BeforeSave;
        }

        private bool BeforeSave(DatabaseEntity arg)
        {
            //alias prefix is ' as ' so make sure user doesn't start a new line with "bobbob\r\nas fish" since this won't be recognised, solve the problem by inserting the original alias
            if (RDMPQuerySyntaxHelper.AliasPrefix.StartsWith(" "))
            {
                string before = QueryEditor.Text;
                string after = Regex.Replace(before, "^" + RDMPQuerySyntaxHelper.AliasPrefix.TrimStart(),
                    RDMPQuerySyntaxHelper.AliasPrefix, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                if (!before.Equals(after))
                    QueryEditor.Text = after;
            }

            SubstituteQueryEditorTextIfContainsLineComments();

            return true;
        }


        private void QueryEditorOnTextChanged(object sender, EventArgs eventArgs)
        {
            try
            {
                string sql;
                string alias;
                
                //ensure it's all on one line
                RDMPQuerySyntaxHelper.SplitLineIntoSelectSQLAndAlias(QueryEditor.Text, out sql, out alias);

                ExtractionInformation.SelectSQL = sql;
                ExtractionInformation.Alias = alias;

                RDMPQuerySyntaxHelper.CheckSyntax(ExtractionInformation);
                ExtractionInformation.GetRuntimeName();
                ragSmiley1.Reset();
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
            }
        }

        public override void SetDatabaseObject(IActivateItems activator,ExtractionInformation databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);

            Setup(databaseObject);
            objectSaverButton1.SetupFor(databaseObject,_activator.RefreshBus);
            objectSaverButton1.BeforeSave += objectSaverButton1OnBeforeSave;
        }

        private bool objectSaverButton1OnBeforeSave(DatabaseEntity databaseEntity)
        {
            if (_namesMatchedWhenDialogWasLaunched)
            {
                var cataItem = ExtractionInformation.CatalogueItem;
                if (!cataItem.Name.Equals(ExtractionInformation.GetRuntimeName()))
                    //which now has a different name (usually alias)
                    if (
                        MessageBox.Show(
                            "Rename CatalogueItem " + cataItem.Name + " to match the new Alias? (" +
                            ExtractionInformation.GetRuntimeName() + ")", "Update CatalogueItem name?",
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        cataItem.Name = ExtractionInformation.GetRuntimeName();
                        cataItem.SaveToDatabase();
                    }
            }

            return true;
        }


        private void Setup(ExtractionInformation extractionInformation)
        {
            ExtractionInformation = extractionInformation;

            if (isFirstTimeSetupCalled)
            {
                //if the catalogue item has same name as the extraction information (alias)
                if (ExtractionInformation.CatalogueItem.Name.Equals(ExtractionInformation.ToString()))
                    _namesMatchedWhenDialogWasLaunched = true;

                var autoComplete = new AutoCompleteProviderFactory(_activator).Create(ExtractionInformation);
                autoComplete.RegisterForEvents(QueryEditor);
            }

               
            ddExtractionCategory.Enabled = true;
            ddExtractionCategory.SelectedItem = ExtractionInformation.ExtractionCategory;

            //deal with empty values in database (shouldn't be any but could be)
            if (string.IsNullOrWhiteSpace(ExtractionInformation.SelectSQL))
            {
                ExtractionInformation.SelectSQL = ExtractionInformation.ColumnInfo.Name.Trim();
                ExtractionInformation.SaveToDatabase();
            }


            QueryEditor.Text = ExtractionInformation.SelectSQL + (!string.IsNullOrWhiteSpace(ExtractionInformation.Alias) ? RDMPQuerySyntaxHelper.AliasPrefix + ExtractionInformation.Alias : "");
            lblFromTable.Text = ExtractionInformation.ColumnInfo.TableInfo.Name;

            tbDefaultOrder.Text = ExtractionInformation.Order.ToString();
            tbAlias.Text = ExtractionInformation.Alias;

            cbHashOnDataRelease.Enabled = true;
            cbHashOnDataRelease.Checked = ExtractionInformation.HashOnDataRelease;

            cbIsExtractionIdentifier.Enabled = true;
            cbIsExtractionIdentifier.Checked = ExtractionInformation.IsExtractionIdentifier;

            cbIsPrimaryKey.Enabled = true;
            cbIsPrimaryKey.Checked = ExtractionInformation.IsPrimaryKey;

            if (!pSql.Controls.Contains(QueryEditor))
                pSql.Controls.Add(QueryEditor);
            
        }
        
        private void ddExtractionCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            //user changed the category so update the object and indicate that we have now got unsaved changes
            if (ddExtractionCategory.SelectedItem != null && ExtractionInformation != null)
                ExtractionInformation.ExtractionCategory = (ExtractionCategory) ddExtractionCategory.SelectedItem;
        }
        
        private void cbHashOnDataRelease_CheckedChanged(object sender, EventArgs e)
        {
            ExtractionInformation.HashOnDataRelease = cbHashOnDataRelease.Checked;
            CreateAliasIfDoesntExist(ExtractionInformation);
            
        }

        private void cbIsExtractionIdentifier_CheckedChanged(object sender, EventArgs e)
        {
            ExtractionInformation.IsExtractionIdentifier = cbIsExtractionIdentifier.Checked;
        }

        private void cbIsPrimaryKey_CheckedChanged(object sender, EventArgs e)
        {
            ExtractionInformation.IsPrimaryKey = cbIsPrimaryKey.Checked;
        }

        private void CreateAliasIfDoesntExist(ExtractionInformation extractionInformation)
        {
            if (string.IsNullOrWhiteSpace(ExtractionInformation.Alias))
            {
                ExtractionInformation.Alias = ExtractionInformation.GetRuntimeName();
                Setup(extractionInformation);
            }
        }
        /// <summary>
        /// Scans the query text for line comments and replaces any with block comments so the query will still work when flattened to a single line
        /// </summary>
        private void SubstituteQueryEditorTextIfContainsLineComments()
        {
            // regex:
            // \s* = don't capture whitespace before or after the comment so we can consistently add a single space front and back for the block comment
            // .*? = lazy capture of comment text, so we don't eat repeated whitespace at the end of the comment (matched by the second \s* outside the capture group)
            var commentRegex = new Regex(@"--\s*(?<comment>.*?)\s*" + Environment.NewLine);
            if (commentRegex.Matches(QueryEditor.Text).Count > 0)
            {
                MessageBox.Show("Line comments are not allowed in the query, these will be automatically converted to block comments.", "Line comments");
                QueryEditor.Text = commentRegex.Replace(QueryEditor.Text, "/* ${comment} */" + Environment.NewLine);
            }
        }
        
        public void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;

            if (ExtractionInformation == null)
                return;

            if (string.IsNullOrWhiteSpace(ExtractionInformation.Alias) && ExtractionInformation.HashOnDataRelease)
            {
                MessageBox.Show(
                    "You must put in an Alias ( AS XYZ ) at the end of your query if you want to hash it on extraction (to a researcher)");
                e.Cancel = true;
                return;
            }

            if (ExtractionInformation.Exists())
                if (OfferChanceToSaveDialog.ShowIfRequired(ExtractionInformation) == DialogResult.Yes)
                    _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(ExtractionInformation));
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        public override string GetTabName()
        {
            return ExtractionInformation.GetRuntimeName() + " (" + ExtractionInformation.CatalogueItem.Catalogue.Name +" Extraction Logic)";
        }
    }
    
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionInformationUI_Design, UserControl>))]
    public abstract class ExtractionInformationUI_Design : RDMPSingleDatabaseObjectControl<ExtractionInformation>
    {
    }
}
