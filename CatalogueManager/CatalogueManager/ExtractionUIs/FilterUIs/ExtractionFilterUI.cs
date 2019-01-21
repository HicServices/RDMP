using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueManager.AutoComplete;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataViewing;
using CatalogueManager.ExtractionUIs.FilterUIs.Options;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable.Revertable;
using MapsDirectlyToDatabaseTableUI;
using CatalogueManager.Copying;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.ExtractionUIs.FilterUIs
{
    /// <summary>
    /// One major problem with research data curation/provision is that data analysts who routinely work with datasets build up an in-depth knowledge of the data and how to identify 
    /// interesting subsets (e.g. 'How to identify all lab test codes for Creatinine').  This can involve complicated SQL which can end up buried in undocumented extraction scripts 
    /// or kept in the head of the data analyst and lost if he ever leaves the organisation.
    /// 
    /// <para>RDMP Filters are an attempt to reduce this risk by centralising SQL 'WHERE' logic into clearly defined and documented reusable blocks (called Filters).  These named filters can
    /// then be combined/used by new data analyst who don't necessarily understand the exact implementation.  For this to work it is vital that you accurately name and describe what each
    /// filter does, including any peculiarities and that you robustly test the SQL in the implementation to make sure it actually works.</para>
    /// 
    /// <para>To write the actual implementation type into the SQL prompt (omitting the 'WHERE' keyword).  For example you could create a Filter called 'Active Records Only' with a description
    /// 'This filter throws out all records which have expired because a clinician has deleted them or the patient has withdrawn consent' and implmenetation SQL of 'MyTable.IActive=1'. Make
    /// sure to fully specify the names of columns in your WHERE SQL incase the filter is used as part of a join across multiple tables with columns that contain the same name (e.g. it might
    /// be that many other tables also include a field called IsActive).  Make sure you fully explore your dataset before finalising your filter and consider edge cases e.g. what does it mean
    /// when IsActive is null? are there any values above 1? and if so what does that mean?</para>
    /// 
    /// <para>If you want to parameterise your query (e.g. a filter for 'Approved name of drug like X') then just type a parameter like you normally would e.g. 'Prescription.DrugName like @drugName'
    /// and save. This will automatically create an empty parameter (See ParameterCollectionUI).</para>
    /// </summary>
    public partial class ExtractionFilterUI :ExtractionFilterUI_Design, ILifetimeSubscriber, ISaveableUI
    {
        private IFilter _extractionFilter;
        public IFilter ExtractionFilter
        {
            get { return _extractionFilter; }
            private set
            {
                _extractionFilter = value;
                
                FigureOutGlobalsAndAutoComplete(value);

                tbFilterName.Text = value.Name;
                tbFilterDescription.Text = value.Description;
                QueryEditor.Text = value.WhereSQL;
                cbIsMandatory.Checked = value.IsMandatory;
                
                //if we are not looking at a catalogue filter (ExtractionFilter) we must be looking at an AggregateFilter 
                //or Deployed filter but it could be a new one and not a clone - if it is a novel user creation, let him 
                //publish it if he wants
                btnPublishToCatalogue.Enabled = !(value is ExtractionFilter) && Catalogue != null;

                RunChecksIfFilterIsICheckable();
            }
        }
        
        private AutoCompleteProvider _autoCompleteProvider;

        public ISqlParameter[] GlobalFilterParameters { get; private set; }

        private Scintilla QueryEditor;
   
        public ExtractionFilterUI()
        {
            InitializeComponent();

            #region Query Editor setup

            if (VisualStudioDesignMode) //dont add the QueryEditor if we are in design time (visual studio) because it breaks
                return;

            QueryEditor = new ScintillaTextEditorFactory().Create(new RDMPCommandFactory());
            QueryEditor.TextChanged += QueryEditor_TextChanged;
            pQueryEditor.Controls.Add(QueryEditor);
            #endregion QueryEditor

            
            objectSaverButton1.BeforeSave += BeforeSave;
            objectSaverButton1.AfterSave += AfterSave;

            autocompleteReminder.Setup("Show Objects",Keys.Control,Keys.Space);
        }

        void QueryEditor_TextChanged(object sender, EventArgs e)
        {
            ExtractionFilter.WhereSQL = QueryEditor.Text;
        }

        private void FigureOutGlobalsAndAutoComplete(IFilter value)
        {
            var factory = new FilterUIOptionsFactory();
            var options = factory.Create(value);
            
            var autoCompleteFactory = new AutoCompleteProviderFactory(_activator);
            _autoCompleteProvider = autoCompleteFactory.Create(value.GetQuerySyntaxHelper());
            
            foreach (var t in options.GetTableInfos())
                _autoCompleteProvider.Add(t);

            foreach (var c in options.GetIColumnsInFilterScope())
                _autoCompleteProvider.Add(c);

            GlobalFilterParameters = options.GetGlobalParametersInFilterScope();

            foreach (ISqlParameter parameter in GlobalFilterParameters)
                _autoCompleteProvider.Add(parameter);

            _autoCompleteProvider.RegisterForEvents(QueryEditor);
        }
        

        /// <summary>
        /// Gives the user an option to save the changes to the filter (if they have unsaved changes) call things for example when closing the host form.
        /// </summary>
        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            if (ExtractionFilter != null && ExtractionFilter.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent)
                if (DialogResult.Yes ==
                    MessageBox.Show(
                        "You have unsaved changes to Filter \"" + ExtractionFilter.Name +
                        "\", would you like to save these now?", "Save Changes to Filter?", MessageBoxButtons.YesNo))
                    objectSaverButton1.Save();
                else
                {
                    try
                    {
                        //So there are local changes to the filter but the user doesnt want to save them.  We need to undo the local changes to the
                        //object that we have a reference to.  This is important because other classes might still have references to that object too
                        //so we fetch a fresh copy out of the database (RevertChanges) and set each of the properties to the original (last saved) values
                        ExtractionFilter.RevertToDatabaseState();
                    }
                    catch (Exception ex)
                    {
                        ExceptionViewer.Show("Failed to revert changes on filter, did you delete it?",ex);
                    }
                }
        }

        private void tbFilterName_TextChanged(object sender, EventArgs e)
        {
            int caretPosition = tbFilterName.SelectionStart;

            if(string.IsNullOrWhiteSpace(tbFilterName.Text))
            {
                tbFilterName.Text = "No Name";
                tbFilterName.SelectAll();
            }

            ExtractionFilter.Name = tbFilterName.Text;
            tbFilterName.SelectionStart = caretPosition;   
        }

        private void tbFilterDescription_TextChanged(object sender, EventArgs e)
        {
            int caretPosition = tbFilterDescription.SelectionStart;
            ExtractionFilter.Description = tbFilterDescription.Text;
            tbFilterDescription.SelectionStart = caretPosition;    
        }

        private bool BeforeSave(DatabaseEntity databaseEntity)
        {
            SubstituteQueryEditorTextIfContainsLineComments();
            OfferWrappingIfUserIncludesANDOrOR();

            //update SQL
            ExtractionFilter.WhereSQL = QueryEditor.Text.TrimEnd();
            
            var creator = new ParameterCreator(ExtractionFilter.GetFilterFactory(), GlobalFilterParameters, null);
            creator.CreateAll(ExtractionFilter,null);

            return true;
        }

        private void AfterSave()
        {
            RunChecksIfFilterIsICheckable();
        }

        private void OfferWrappingIfUserIncludesANDOrOR()
        {

            if (QueryEditor.Text.ToLower().Contains(" and ") || QueryEditor.Text.ToLower().Contains(" or "))
            {
                //user is creating a filter with boolean logic in it! better wrap their function if it isn't already
                if(QueryEditor.Text.Trim().StartsWith("("))//it already does so no worries
                    return;

                MessageBox.Show("Your Filter SQL has an AND / OR in it, so we are going to wrap it in brackets for you", "Filter contains AND/OR");

                QueryEditor.Text = "(" + QueryEditor.Text + ")";

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
                MessageBox.Show("Line comments are not allowed in the filter query, these will be automatically converted to block comments.", "Line comments");
                QueryEditor.Text = commentRegex.Replace(QueryEditor.Text, "/* ${comment} */" + Environment.NewLine);
            }
        }

        private void RunChecksIfFilterIsICheckable()
        {
            var checkable = ExtractionFilter as ICheckable;

            if (checkable != null)
                ragSmiley1.StartChecking(checkable);
        }

        public override void SetDatabaseObject(IActivateItems activator, ConcreteFilter databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            Catalogue = databaseObject.GetCatalogue();
            ExtractionFilter = databaseObject;

            objectSaverButton1.SetupFor((DatabaseEntity)ExtractionFilter,_activator.RefreshBus);

            Add(new ExecuteCommandViewFilterMatchData(_activator, databaseObject, ViewType.TOP_100));
            Add(new ExecuteCommandViewFilterMatchData(_activator,databaseObject,ViewType.Aggregate));
            Add(new ExecuteCommandViewFilterMatchGraph(_activator,databaseObject));
            Add(new ExecuteCommandViewSqlParameters(_activator,databaseObject));
            Add(new ExecuteCommandBrowseLookup(_activator,databaseObject));
        }
       
        
        private void cbIsMandatory_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                ExtractionFilter.IsMandatory = cbIsMandatory.Checked;
                ExtractionFilter.SaveToDatabase();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        

        /// <summary>
        /// Used for publishing IFilters created here back to the main Catalogue
        /// </summary>
        public Catalogue Catalogue { get; set; }

        private void btnPublishToCatalogue_Click(object sender, EventArgs e)
        {
            if(Catalogue == null)
            {
                WideMessageBox.Show("Unexpected system state","Unsure how you manged to click this button when Catalogue was null!");
                return;
            } 
            if (ExtractionFilter == null)
            {
                WideMessageBox.Show("Unexpected system state", "Unsure how you managed to click this button when ExtractionFilter was null!");
                return;
            }
            
            ExtractionInformation[] allExtractionInformations = Catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
            
            if (!allExtractionInformations.Any())
            {
                WideMessageBox.Show("Cannot publish filter", "Cannot publish filter because Catalogue " + Catalogue +
                                    " does not have any ExtractionInformations (extractable columns) we could associate it with");
                return;
            }
            
            string reason;
            if (!FilterImporter.IsProperlyDocumented(ExtractionFilter,out reason))
            {
                WideMessageBox.Show("Cannot publish filter", "Filter is not properly documented:" + reason);
                return;
            }
            
            var dr = MessageBox.Show(
                "You are about to commit filter '" + ExtractionFilter + "' as new master ExtractionFilter in the Catalogue '" +
                Catalogue +
                "', this will make it reusable asset for anyone using the dataset anywhere, is that what you want? (You will be prompted to pick a column to associate the new master filter with)",
                "Create new master ExtractionFilter?", MessageBoxButtons.YesNoCancel);

            if (dr == DialogResult.Yes)
            {
                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(allExtractionInformations,false,false);
                dialog.ShowDialog();

                if (dialog.DialogResult == DialogResult.OK && dialog.Selected != null)
                {
                    btnPublishToCatalogue.Enabled = false;

                    var toAddTo = (ExtractionInformation) dialog.Selected;
                    
                    //see if there is one with the same name that for some reason we are not known to be a child of already
                    var duplicate = toAddTo.ExtractionFilters.SingleOrDefault(f => f.Name.Equals(ExtractionFilter.Name));

                    if (duplicate != null)
                    {
                        var drMarkSame = MessageBox.Show("There is already a filter called " + ExtractionFilter.Name +
                                                            " in ExtractionInformation " + toAddTo + " do you want to mark this filter as a child of that master filter?",
                                                                "Duplicate, mark these as the same?",MessageBoxButtons.YesNo);

                        if (drMarkSame == DialogResult.Yes)
                        {
                            ExtractionFilter.ClonedFromExtractionFilter_ID = duplicate.ID;
                            ExtractionFilter.SaveToDatabase();
                        }
                        return;
                    }


                    new FilterImporter(new ExtractionFilterFactory(toAddTo), null).ImportFilter(ExtractionFilter, null);
                    MessageBox.Show("Publish successful");
                }
            }
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            if(!(e.Object is IFilter))
                return;
            
            if(e.Object.Equals(ExtractionFilter))
                if (!e.Object.Exists()) //its deleted
                    this.ParentForm.Close();
                else
                    ExtractionFilter = (IFilter) e.Object;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionFilterUI_Design, UserControl>))]
    public abstract class ExtractionFilterUI_Design : RDMPSingleDatabaseObjectControl<ConcreteFilter>
    {
    }
}
