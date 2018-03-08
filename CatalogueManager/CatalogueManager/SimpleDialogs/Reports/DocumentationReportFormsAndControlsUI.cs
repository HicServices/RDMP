using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.SimpleDialogs.Reports
{
    /// <summary>
    /// Generates documentation including screenshots of every Form and Control in the RDMP codebase (Based on SourceCodeForSelfAwareness.zip).  Reflection is used to identify all inheritors
    /// of Form and UserControl.  The source code is evaluated for summary tags (like this one).  The form/control is constructed using all null parameters and a screenshot is taken of the 
    /// control.
    /// 
    /// You can also see any errors in the generation of the screenshot or documentation in the ChecksUI. 
    /// </summary>
    public partial class DocumentationReportFormsAndControlsUI : RDMPForm
    {
        private readonly IActivateItems _activator;
        private bool _grabArbitraryDatabaseObjects = false;
        

        public DocumentationReportFormsAndControlsUI(IActivateItems activator)
        {
            _activator = activator;
            InitializeComponent();
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            Dictionary<string,string> namespaceToApplication = new Dictionary<string, string>();

            namespaceToApplication.Add("ResearchDataManagementPlatform", "Dataset Management");
            namespaceToApplication.Add("CatalogueManager", "Dataset Management");
            namespaceToApplication.Add("ReusableUIComponents", "Dataset Management");
            namespaceToApplication.Add("Diagnostics", "Dataset Management");
            namespaceToApplication.Add("LoadModules", "Dataset Management");
            namespaceToApplication.Add("MapsDirectlyToDatabaseTableUI", "Dataset Management");
            namespaceToApplication.Add("RDMPObjectVisualisation", "Dataset Management");

            namespaceToApplication.Add("DataExportManager", "Data Export Management");
            
            namespaceToApplication.Add("DatasetLoaderUI", "Data Loading");
            namespaceToApplication.Add("DataLoadEngine", "Data Loading");

            namespaceToApplication.Add("Dashboard", "Dataset Management");
            namespaceToApplication.Add("CohortManager", "Cohort Management");
            namespaceToApplication.Add("CachingUI", "Caching Management");

            //Special, these are controls the user doesn't want to know about, put them at the end
            namespaceToApplication.Add("TechnicalUI", "Technical");

            Dictionary<string, List<Type>> applicationToClasses = new Dictionary<string, List<Type>>();

            var d = new DirectoryInfo(Environment.CurrentDirectory);
            
            //try to load all dlls and exes in the current directory
            foreach (var dll in d.GetFiles("*.exe").Union(d.GetFiles("*.dll")))
            {
                try
                {
                    Assembly.LoadFrom(dll.FullName);
                    checksUI1.OnCheckPerformed(new CheckEventArgs("Loaded file " + dll.Name, CheckResult.Success));
                }
                catch (Exception exception)
                {
                    checksUI1.OnCheckPerformed(new CheckEventArgs("Failed to load file " + dll.Name, CheckResult.Fail,exception));
                }
            }
            
            try
            {
                var types = GetAllFormsAndControlTypes();
                
                foreach (KeyValuePair<string, string> kvp in namespaceToApplication)
                    if(!applicationToClasses.ContainsKey(kvp.Value))
                    applicationToClasses.Add(kvp.Value, new List<Type>());

                foreach (Type type in types)
                {
                    string fullname = type.FullName;

                    if (fullname.Contains('.'))
                        fullname = type.FullName.Substring(0, type.FullName.IndexOf('.'));

                    //if it has the TechnicalUI attribute add it in the Technical chapter
                    if(Attribute.GetCustomAttribute(type, typeof (TechnicalUI)) != null)
                        applicationToClasses[namespaceToApplication["TechnicalUI"]].Add(type);
                    else
                    //otherwise add it in the Chapter that relates to user experience
                    if(namespaceToApplication.ContainsKey(fullname))
                        applicationToClasses[namespaceToApplication[fullname]].Add(type);
                    else
                    {
                        checksUI1.OnCheckPerformed(
                            new CheckEventArgs("Did not know which application class belongs to:" + type.FullName,
                                CheckResult.Warning));
                    }
                }
                
                var office = new DocumentationReportFormsAndControlsOfficeBit();
                var images = new EnumImageCollection<RDMPConcept>(CatalogueIcons.ResourceManager).ToStringDictionary(12);


                Dictionary<string,Bitmap> icons = new Dictionary<string, Bitmap>();
                foreach (RDMPConcept c in Enum.GetValues(typeof (RDMPConcept)))
                {
                    //things we don't want to show icons for because it would be confusing
                    if(c == RDMPConcept.SQL || c == RDMPConcept.Clipboard || c == RDMPConcept.File || c == RDMPConcept.Help || c== RDMPConcept.Release || c == RDMPConcept.Database || c== RDMPConcept.Filter || c == RDMPConcept.Logging || c == RDMPConcept.DQE)
                        continue;

                    icons.Add(c.ToString(), _activator.CoreIconProvider.GetImage(c));
                }

                office.GenerateReport(checksUI1, applicationToClasses, GetImagesForType, images, icons);
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Total failure", CheckResult.Fail, exception));
            }
        }

        public IEnumerable<Type> GetAllFormsAndControlTypes()
        {
            List<Exception> ex;
            return RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex)
                .Where(
                t =>
                    (typeof(Form).IsAssignableFrom(t) || typeof(UserControl).IsAssignableFrom(t))
                    &&
                    !t.FullName.StartsWith("Microsoft")
                    &&
                    !t.FullName.StartsWith("System")
                    ).ToArray();

        }

        private Bitmap GetImagesForType(Type t)
        {
            try
            {
                if (t.IsAbstract)
                    return null;

                //try constructing it
                foreach (ConstructorInfo constructor in t.GetConstructors())
                {
                    try
                    {
                        //null parameters!
                        object[] p = new object[constructor.GetParameters().Count()];

                        //instantiate the control
                        Control c = (Control) constructor.Invoke(p);
                        
                        //propagate the repository locator and VisualStudioDesignMode = true downwards
                        ServiceLocatorPropagatorToChildControls propagator = new ServiceLocatorPropagatorToChildControls(RepositoryLocator);
                        propagator.PropagateRecursively(new[] {c},true);//true = pretend we are visual studio :)

                        Form f = c as Form;
                        
                        //if it isn't a form, create one and put the control on it
                        if (f == null)
                        {
                            //User Control
                            f = new Form();
                            f.ClientSize = new Size(c.PreferredSize.Width, c.PreferredSize.Height);
                            f.Controls.Add(c);
                        }

                        f.FormBorderStyle = FormBorderStyle.FixedSingle;
                        f.Show();

                        try
                        {
                            InitializeWithArbitraryObjectIfSingleDatabaseObjectControl(t, c);

                            InitializeIfToolbox(t, c);
                            f.Invalidate(true);

                            return AddImageOf(c);
                        }
                        finally
                        {
                            f.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        checksUI1.OnCheckPerformed(new CheckEventArgs("Could not use any of the constructors on Type " + t.Name,CheckResult.Fail,ex));
                    }

                }
                
            }
            catch (Exception ex2)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Could not find constructors of Type " + t.Name,CheckResult.Fail,ex2));
            }

            return null;
        }

        private void InitializeIfToolbox(Type type, Control control)
        {
            var collection = control as RDMPCollectionUI;
            if(collection != null)
            {
                collection.SetItemActivator(_activator);
                collection.CommonFunctionality.ExpandOrCollapse(null);
                
                Stopwatch sw = new Stopwatch();
                sw.Start();
            }
        }

        private void InitializeWithArbitraryObjectIfSingleDatabaseObjectControl(Type t, Control c)
        {
            var single = c as IRDMPSingleDatabaseObjectControl;

            if (single != null && _grabArbitraryDatabaseObjects)
            {
                var expectedObjectType = single.GetTypeOfT();

                //specific abstract/interface components here can be downtyped to the most handy object
                if (expectedObjectType == typeof (ConcreteFilter))
                    expectedObjectType = typeof (ExtractionFilter);

                try
                {
                    IMapsDirectlyToDatabaseTable dbObject = null;


                    if (RepositoryLocator.CatalogueRepository.SupportsObjectType(expectedObjectType))
                    {
                        dbObject = RepositoryLocator.CatalogueRepository.GetAllObjects(expectedObjectType).FirstOrDefault();

                        if (dbObject == null)
                            checksUI1.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Could not find an instance of Type '" + expectedObjectType.Name +
                                    "' in CatalogueRepository", CheckResult.Warning));
                    }
                    else if (RepositoryLocator.DataExportRepository.SupportsObjectType(expectedObjectType))
                    {
                        dbObject = RepositoryLocator.DataExportRepository.GetAllObjects(expectedObjectType).FirstOrDefault();

                        if (dbObject == null)
                            checksUI1.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Could not find an instance of Type '" + expectedObjectType.Name +
                                    "' in CatalogueRepository", CheckResult.Warning));
                    }
                    else
                        checksUI1.OnCheckPerformed(
                            new CheckEventArgs(
                                "Neither Catalogue or DataExport repository admitted hosting objects of Type'" +
                                expectedObjectType.Name + "'", CheckResult.Fail));


                    if (dbObject != null)
                        t.GetMethods().Single(m=>m.Name.Equals("SetDatabaseObject")  && m.DeclaringType == t).Invoke(c, new object[] {_activator, dbObject});
                }
                catch (Exception e)
                {
                    checksUI1.OnCheckPerformed(
                        new CheckEventArgs(
                            "Could not initialize Type '" + t.Name + "' with arbitrary database object of Type '" +
                            expectedObjectType.FullName + "'", CheckResult.Fail, e));
                }
            }
        }

        private Bitmap AddImageOf(Control c)
        {

            Bitmap bmp;
            if (c.Width <= 600)
                bmp = new Bitmap(c.Width, c.Height);
            else
            {
                //it's too wide to fit on the word document
                double aspectRatio = 600.0 / c.Width;
                bmp = new Bitmap(600, (int)(c.Height * aspectRatio));
            }


            DrawControlToImage(c, bmp);

            return bmp;
        }

        static void DrawControlToImage(Control ctrl, Image img)
        {

            Rectangle sourceRect = new Rectangle(0,0,ctrl.Width,ctrl.Height);
            
            Size targetSize = new Size(img.Width, img.Height);
            
            using (Bitmap tmp = new Bitmap(sourceRect.Width, sourceRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                ctrl.DrawToBitmap(tmp, sourceRect);
                using (Graphics g = Graphics.FromImage(img))
                {
                    g.DrawImage(tmp, new Rectangle(Point.Empty, targetSize));
                }
            }
        }

        private void cbGrabArbitraryOjbectsToPopulateUIs_CheckedChanged(object sender, EventArgs e)
        {
            _grabArbitraryDatabaseObjects = cbGrabArbitraryOjbectsToPopulateUIs.Checked;
        }
    }
}
